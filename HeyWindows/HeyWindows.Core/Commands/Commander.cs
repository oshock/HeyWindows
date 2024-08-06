using System.Data;
using System.Speech.Recognition;
using HeyWindows.Core.Grammars;
using HeyWindows.Core.Listeners;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Commands;

public class Commander
{
    private readonly List<CommandContainer> _containers = new();
    
    public void InitializeContainer(CommandContainer container)
    {
        if (_listener is null)
            throw new InvalidOperationException("Cannot add commands without a valid listener.");

        _containers.Add(container);
        _listener?.Grammars.Add(container.Commands.BuildGrammarFromCommands());

        _listener?.Initialize(); // Consume added grammars
    }

    private Listener? _listener;

    public void Initialize()
    {
        _listener = new Listener
        { CommandingCommander = this };
    }

    public void Activate() => _listener!.Listen();
    
    public void Execute(string phrase)
    {
        foreach (var container in _containers)
        {
            var commands = container.FindCommands(phrase);
            if (commands.Count == 0)
                continue;

            foreach (var command in commands)
            {
                if (!command.TryExecute())
                    LogError($"'{command.Name}' failed to execute. Check log for more information.");
            }
        }
    }
}