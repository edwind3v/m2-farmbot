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

    public bool TryEnter(ClientSlot slot, GameChannel channel, string password, int timeoutMs, out string status)
    {
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
        Thread.Sleep(100);
        if (!WaitFor(deadline, () =>
            {
                var phase = CurrentPhase();
                if (IsAtLeastLogin(phase))
                    return true;
                _sink.SkipIntro();
                return IsAtLeastLogin(CurrentPhase());
            }))
        {
            status = "timed out waiting for login phase: " + CurrentPhase();
            _log.Warn(status);
            return false;
        }

        _log.Info($"quick play login for {slot.Account} slot {characterSlot + 1} on {channel.Name}");
        var loginError = "";
        if (!WaitFor(deadline, () =>
            {
                if (IsSelectOrEntering(CurrentPhase()) || Snapshot().InWorld)
                    return true;
                var login = _sink.Login(
                    slot.Account,
                    password,
                    channel.Host,
                    channel.Port,
                    channel.AuthHost,
                    channel.AuthPort,
                    characterSlot);
                if (!login.Ok)
                {
                    loginError = string.IsNullOrWhiteSpace(login.Reason) ? "login command failed" : login.Reason;
                    return true;
                }

                return IsSelectOrEntering(CurrentPhase()) || Snapshot().InWorld;
            }) || !string.IsNullOrEmpty(loginError))
        {
            status = string.IsNullOrEmpty(loginError)
                ? "timed out waiting for character select: " + CurrentPhase()
                : loginError;
            _log.Warn(status);
            return false;
        }

        if (NeedsCharacterSelect(CurrentPhase()) && !Snapshot().InWorld)
        {
            _log.Info("quick play waiting for auto DirectEnter from " + CurrentPhase());
            WaitFor(DateTime.UtcNow.AddMilliseconds(8000), () => IsEnteringWorld(CurrentPhase()) || Snapshot().InWorld);
        }

        if (NeedsCharacterSelect(CurrentPhase()) && !Snapshot().InWorld)
        {
            _log.Info("quick play skipping character select slot " + (characterSlot + 1) + " from " + CurrentPhase());
            var select = _sink.SelectCharacter(characterSlot);
            if (!select.Ok)
            {
                status = string.IsNullOrWhiteSpace(select.Reason) ? "select character failed" : select.Reason;
                _log.Error("quick play select: " + status);
                return false;
            }
        }

        if (!WaitFor(deadline, () => IsEnteringWorld(CurrentPhase()) || Snapshot().InWorld))
        {
            status = "timed out waiting for loading: " + CurrentPhase();
            _log.Warn(status);
            return false;
        }

        if (!Snapshot().InWorld)
        {
            _log.Info("quick play waiting for LoadData to finish from " + CurrentPhase());
            if (!WaitFor(deadline, () => Snapshot().InWorld || Snapshot().StartGame))
            {
                status = "timed out waiting for load steps: " + CurrentPhase();
                _log.Warn(status);
                return false;
            }
        }

        if (!Snapshot().InWorld)
        {
            _log.Info("quick play waiting for GameWindow after LoadData from " + CurrentPhase());
            WaitFor(DateTime.UtcNow.AddMilliseconds(5000), () => Snapshot().InWorld);
        }

        if (!Snapshot().InWorld && Snapshot().StartGame)
        {
            _log.Info("quick play SetGamePhase after LoadData from " + CurrentPhase());
            var enter = _sink.EnterGame(setGamePhase: true);
            _log.Info("quick play GameWindow: " + Reason(enter));
        }

        if (!WaitFor(deadline, () => Snapshot().InWorld))
        {
            status = "timed out waiting to enter the world: " + CurrentPhase();
            _log.Warn(status);
            return false;
        }

        var session = _sink.GetSession();
        if (!string.IsNullOrWhiteSpace(session.Character))
            slot.CharacterName = session.Character;
        status = "in game" + (string.IsNullOrWhiteSpace(slot.CharacterName) ? "" : " as " + slot.CharacterName);
        _log.Info("quick play " + status);
        return true;
    }

    private ActionResponse Snapshot() => _sink.GetPhase();

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

    private static bool IsAtLeastLogin(string phase) =>
        ContainsPhase(phase, "Login") || IsSelectOrEntering(phase);

    public static bool NeedsCharacterSelect(string phase) =>
        ContainsPhase(phase, "Select") && !IsEnteringWorld(phase);

    public static bool IsSelectOrEntering(string phase) =>
        NeedsCharacterSelect(phase) || IsEnteringWorld(phase);

    public static bool IsEnteringWorld(string phase) =>
        ContainsPhase(phase, "Loading") || IsGame(phase);

    public static bool IsGame(string phase) =>
        ContainsPhase(phase, "Game") && !ContainsPhase(phase, "Loading");

    private static bool ContainsPhase(string phase, string name) =>
        !string.IsNullOrWhiteSpace(phase) &&
        phase.Contains(name, StringComparison.OrdinalIgnoreCase);
}
