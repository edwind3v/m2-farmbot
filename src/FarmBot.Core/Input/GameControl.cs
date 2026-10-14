using FarmBot.Core.Configuration;
using FarmBot.Core.Ipc;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;

namespace FarmBot.Core.Input;

public interface IGameControl
{
    void Execute(BotCommand command);
}

public sealed class RoutingControl : IGameControl
{
    private readonly PipeHost _pipe;
    private readonly IGameControl _sendInput;
    private readonly Func<BotConfig> _config;
    private readonly IBotLog _log;

    public RoutingControl(PipeHost pipe, IGameControl sendInput, Func<BotConfig> config, IBotLog log)
    {
        _pipe = pipe;
        _sendInput = sendInput;
        _config = config;
        _log = log;
    }

    public void Execute(BotCommand command)
    {
        var config = _config();
        var ipcReady = _pipe.IsConnected && _pipe.SupportsCommands;
        var usePipe = config.InputMode switch
        {
            InputMode.Ipc => true,
            InputMode.SendInput => false,
            _ => ipcReady
        };
        var useSendInput = config.InputMode switch
        {
            InputMode.SendInput => true,
            InputMode.Ipc => false,
            _ => !ipcReady
        };

        if (usePipe)
        {
            if (!_pipe.TrySend(command))
                _log.Warn($"ipc {command.Describe()} was not delivered");
        }

        if (useSendInput)
            _sendInput.Execute(command);
    }
}
