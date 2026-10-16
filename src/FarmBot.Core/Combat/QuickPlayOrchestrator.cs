using FarmBot.Core.Configuration;
using FarmBot.Core.Logging;

namespace FarmBot.Core.Combat;

public sealed class QuickPlayOrchestrator
{
    private readonly AdapterActionSink _sink;
    private readonly IBotLog _log;

    public QuickPlayOrchestrator(AdapterActionSink sink, IBotLog log)
    {
        _sink = sink;
        _log = log;
    }

    public bool TryEnter(
        ClientSlot slot,
        GameChannel channel,
        string password,
        int timeoutMs,
        out string status,
        Action<string>? progress = null)
    {
        void Report(string message) => progress?.Invoke(message);

        if (string.IsNullOrWhiteSpace(slot.Account) || string.IsNullOrWhiteSpace(password))
        {
            status = "account and password are required for Quick Play";
            return false;
        }

        if (!_sink.Available && !_sink.TryConnect(800))
        {
            status = "companion is not connected";
            _log.Error(status);
            return false;
        }

        var characterSlot = Math.Clamp(slot.CharacterSlot, 0, 4);
        var deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(timeoutMs, 30000));
        Report("waiting for login...");
        Thread.Sleep(500);
        var started = DateTime.UtcNow;
        var lastSkip = DateTime.MinValue;
        var channelsReady = false;
        if (!WaitFor(deadline, () =>
            {
                var phase = CurrentPhase();
                if (IsLoginScreen(phase) || IsSelectOrEntering(phase) || ReallyInWorld())
                    return true;
                var waited = DateTime.UtcNow - started;
                // Baerim often stays OffLine while LoginWindow is already up. Once
                // live channels dump, native login can SetMarkServer+Connect safely.
                if (!channelsReady && waited >= TimeSpan.FromSeconds(2))
                {
                    var live = _sink.GetChannels();
                    channelsReady = LiveChannelList.FromResponse(live).Count > 0;
                    if (channelsReady)
                    {
                        _log.Info("quick play channels ready; proceeding to login (phase=" + phase + ")");
                        return true;
                    }
                }
                if (waited < TimeSpan.FromSeconds(3))
                    return false;
                if (DateTime.UtcNow - lastSkip < TimeSpan.FromMilliseconds(1500))
                    return false;
                lastSkip = DateTime.UtcNow;
                var skip = _sink.SkipIntro();
                var after = !string.IsNullOrWhiteSpace(skip.Phase) ? skip.Phase : CurrentPhase();
                _log.Info("quick play skip_intro from " + phase + " -> " + after + " reason=" + (skip.Reason ?? ""));
                return IsLoginScreen(after) || IsSelectOrEntering(after) || ReallyInWorld() || channelsReady;
            }))
        {
            status = "timed out waiting for login phase: " + CurrentPhase();
            _log.Warn(status);
            return false;
        }

        Thread.Sleep(600);
        var phase = CurrentPhase();
        // Only skip account login when already on select/loading/game. OffLine/HandShake still need Login().
        if (IsSelectOrEntering(phase) || ReallyInWorld())
        {
            _log.Info("quick play skip login, already " + phase);
        }
        else
        {
            Report("logging in...");
            var host = channel.Host;
            var port = channel.Port;
            var authHost = channel.AuthHost;
            var authPort = channel.AuthPort;
            var channelIndex = channel.Index;
            if (LiveChannelList.IsLoopback(host) || string.IsNullOrWhiteSpace(host) || authPort <= 0)
            {
                Report("reading live channels...");
                var live = _sink.GetChannels();
                var channels = LiveChannelList.FromResponse(live);
                if (channels.Count > 0)
                {
                    var pick = channels.FirstOrDefault(c => c.Index == slot.ChannelIndex) ?? channels[0];
                    host = pick.Host;
                    port = pick.Port;
                    authHost = pick.AuthHost;
                    authPort = pick.AuthPort;
                    channelIndex = pick.Index;
                    channel = pick;
                }
                else
                {
                    host = "";
                    port = 0;
                    authHost = "";
                    authPort = 0;
                    _log.Info("quick play deferring host resolve to native login (serverInfo dump empty)");
                }
            }

            if (authPort <= 0 || authPort == port)
                authPort = 30001;
            if (string.IsNullOrWhiteSpace(authHost))
                authHost = host;

            var loginOk = false;
            var lastLoginError = "";
            while (DateTime.UtcNow < deadline)
            {
                phase = CurrentPhase();
                if (IsSelectOrEntering(phase) || ReallyInWorld())
                {
                    loginOk = true;
                    break;
                }

                _log.Info($"quick play login for {slot.Account} slot {characterSlot + 1} on {channel.Name} {host}:{port} auth={authHost}:{authPort} (phase={phase})");
                var login = _sink.Login(
                    slot.Account,
                    password,
                    host,
                    port,
                    authHost,
                    authPort,
                    characterSlot,
                    channelIndex);
                if (login.Ok)
                {
                    loginOk = true;
                    break;
                }

                lastLoginError = string.IsNullOrWhiteSpace(login.Reason) ? "login command failed" : login.Reason;
                var waiting =
                    IsHandshake(lastLoginError) ||
                    IsLoginScreen(lastLoginError) ||
                    lastLoginError.Contains("waiting", StringComparison.OrdinalIgnoreCase) ||
                    lastLoginError.Contains("Off", StringComparison.OrdinalIgnoreCase) ||
                    lastLoginError.Contains("not executed", StringComparison.OrdinalIgnoreCase) ||
                    lastLoginError.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                    lastLoginError.Contains("login sent", StringComparison.OrdinalIgnoreCase) ||
                    lastLoginError.Contains("connect failed", StringComparison.OrdinalIgnoreCase);
                if (!waiting)
                {
                    status = lastLoginError;
                    _log.Warn(status);
                    return false;
                }

                _log.Info("quick play login deferred: " + lastLoginError);
                Thread.Sleep(500);
            }

            if (!loginOk)
            {
                status = string.IsNullOrWhiteSpace(lastLoginError)
                    ? "timed out during login: " + CurrentPhase()
                    : lastLoginError;
                _log.Warn(status);
                return false;
            }
        }

        Report("waiting for character select...");
        if (!WaitFor(deadline, () => IsSelectOrEntering(CurrentPhase()) || ReallyInWorld()))
        {
            status = "timed out waiting for character select: " + CurrentPhase();
            _log.Warn(status);
            return false;
        }

        if (NeedsCharacterSelect(CurrentPhase()) && !ReallyInWorld())
        {
            // SelectSlot -> StartGame only. LoadingWindow then sends SelectCharacterPacket.
            // Bare net.DirectEnter skips StartGame and crashes in Loading.
            Report("selecting character...");
            _log.Info("quick play selecting character slot " + (characterSlot + 1) + " from " + CurrentPhase());
            var selectAttempts = 0;
            while (NeedsCharacterSelect(CurrentPhase()) && !ReallyInWorld() && DateTime.UtcNow < deadline)
            {
                var select = _sink.SelectCharacter(characterSlot);
                selectAttempts++;
                var reason = select.Reason ?? "";
                _log.Info("quick play select attempt " + selectAttempts + ": ok=" + select.Ok + " " + reason);
                if (IsLoginScreen(reason) || IsLoginScreen(CurrentPhase()))
                {
                    Thread.Sleep(400);
                    continue;
                }
                // Only leave the loop after StartGame ("select sent") or phase left Select.
                // Do not treat ok+still-Select from a bad DirectEnter as success.
                if (reason.Contains("select sent", StringComparison.OrdinalIgnoreCase) ||
                    reason.Contains("already entering", StringComparison.OrdinalIgnoreCase) ||
                    IsEnteringWorld(CurrentPhase()) ||
                    ReallyInWorld())
                    break;
                if (select.Ok && !NeedsCharacterSelect(CurrentPhase()))
                    break;
                Thread.Sleep(400);
            }
            // StartGame schedules DirectEnter after SELECT_CHAR_NO_DELAY (~0.5s).
            Thread.Sleep(1200);
        }

        Report("entering the world...");
        // Do not force SetGamePhase — LoadingWindow.__StartGame opens GameWindow itself.
        // Interfering mid-load is what black-screens / crashes Baerim.
        var loginSince = DateTime.MinValue;
        if (!WaitFor(deadline, () =>
            {
                var phase = CurrentPhase();
                if (ReallyInWorld() || IsEnteringWorld(phase))
                {
                    loginSince = DateTime.MinValue;
                    return true;
                }
                if (IsLoginScreen(phase))
                {
                    if (loginSince == DateTime.MinValue)
                        loginSince = DateTime.UtcNow;
                    // Brief Login flicker is ok; stable Login means select failed.
                    return (DateTime.UtcNow - loginSince) > TimeSpan.FromSeconds(2);
                }
                loginSince = DateTime.MinValue;
                return false;
            }))
        {
            status = "timed out waiting for loading: " + CurrentPhase();
            _log.Warn(status);
            return false;
        }
        if (IsLoginScreen(CurrentPhase()) && !ReallyInWorld() && !IsEnteringWorld(CurrentPhase()))
        {
            status = "character select kicked back to login";
            _log.Warn(status);
            return false;
        }

        if (!WaitFor(deadline, () => ReallyInWorld()))
        {
            status = "timed out waiting to enter the world: " + CurrentPhase();
            _log.Warn(status);
            return false;
        }

        var session = _sink.GetSession();
        var names = LiveChannelList.CharacterNamesFromResponse(session);
        if (names.Count > 0)
            SlotStore.ApplyCharacterNames(slot, names);
        if (!string.IsNullOrWhiteSpace(session.Character))
            slot.CharacterName = session.Character;
        status = "in game" + (string.IsNullOrWhiteSpace(slot.CharacterName) ? "" : " as " + slot.CharacterName);
        _log.Info("quick play " + status);
        return true;
    }

    private ActionResponse Snapshot() => _sink.GetPhase();

    private bool ReallyInWorld()
    {
        var response = Snapshot();
        return LooksInWorld(
            response.Phase,
            response.InWorld,
            response.Character,
            response.Level,
            response.MaxHp);
    }

    private string CurrentPhase()
    {
        var response = Snapshot();
        if (!string.IsNullOrWhiteSpace(response.Phase))
            return response.Phase;
        return response.Reason ?? "";
    }

    private static string Reason(ActionResponse response)
    {
        var text = string.IsNullOrWhiteSpace(response.Reason) ? (response.Ok ? "ok" : "failed") : response.Reason;
        return text + " inWorld=" + response.InWorld + " phase=" + (response.Phase ?? "");
    }

    private static bool WaitFor(DateTime deadline, Func<bool> ready)
    {
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (ready())
                    return true;
            }
            catch
            {
                // Companion may still be coming up during logo.
            }

            Thread.Sleep(50);
        }

        try
        {
            return ready();
        }
        catch
        {
            return false;
        }
    }

    public static bool IsHandshake(string phase) =>
        ContainsPhase(phase, "Handshake");

    public static bool IsLoginScreen(string phase) =>
        ContainsPhase(phase, "Login") &&
        !ContainsPhase(phase, "OffLine") &&
        !IsHandshake(phase);

    public static bool IsAtLeastLogin(string phase) =>
        IsLoginScreen(phase) || IsHandshake(phase) || IsSelectOrEntering(phase);

    public static bool NeedsCharacterSelect(string phase) =>
        ContainsPhase(phase, "Select") && !IsEnteringWorld(phase);

    public static bool IsSelectOrEntering(string phase) =>
        NeedsCharacterSelect(phase) || IsEnteringWorld(phase);

    public static bool IsEnteringWorld(string phase)
    {
        if (IsGame(phase))
            return false;
        // Native Baerim reports "Load"; stock Metin2 uses "Loading".
        if (ContainsPhase(phase, "Loading"))
            return true;
        return ContainsPhase(phase, "Load") && !ContainsPhase(phase, "Login");
    }

    public static bool IsGame(string phase) =>
        ContainsPhase(phase, "Game") &&
        !ContainsPhase(phase, "Loading") &&
        !string.Equals(phase, "Load", StringComparison.OrdinalIgnoreCase);

    public static bool LooksInWorld(string? phase, bool inWorld, string? character, int level, int maxHp)
    {
        if (IsGame(phase ?? "") || inWorld)
            return true;
        if (ContainsPhase(phase ?? "", "Login") || ContainsPhase(phase ?? "", "Select") ||
            ContainsPhase(phase ?? "", "Handshake"))
            return false;
        // Sticky native "Load" after char select must not block UI once we have a character.
        if (IsEnteringWorld(phase ?? "") && string.IsNullOrWhiteSpace(character))
            return false;
        if (IsEnteringWorld(phase ?? "") && !string.IsNullOrWhiteSpace(character))
            return true;
        return maxHp > 0 && level is >= 1 and <= 250 && !string.IsNullOrWhiteSpace(character);
    }

    private static bool ContainsPhase(string phase, string name) =>
        !string.IsNullOrWhiteSpace(phase) &&
        phase.Contains(name, StringComparison.OrdinalIgnoreCase);
}
