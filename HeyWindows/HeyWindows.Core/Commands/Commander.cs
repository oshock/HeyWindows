using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Grammars;
using HeyWindows.Core.Listeners;

namespace HeyWindows.Core.Commands;

public class Commander
{
    public static Dictionary<string, ICommandExecutor> ExecutorByName = new()
    {
        {
            "Executable",
            new ExecutableExecutor()
        }
    };
    
    private readonly List<CommandContainer> _containers = new();
    
    public void InitializeContainer(CommandContainer container)
    {
        if (_listener is null)
            throw new InvalidOperationException("Cannot add commands without a valid listener.");

        _containers.Add(container);
        
        if (container.Commands.Count > 0)
            _listener?.Grammars.Add(container.Commands.BuildGrammarFromCommands());

        _listener?.Initialize(); // Consume added grammars
    }

    public void InitializeCommand(Command command)
    {
        var container = new CommandContainer(command.Id, command);
        InitializeContainer(container);
    }

    private Listener? _listener;

    public void Initialize()
    {
        _listener = new Listener
        { CommandingCommander = this };
    }

    public void Activate() => _listener!.Listen();
    
    public void Execute(string phrase, string pronunciation)
    {
        foreach (var container in _containers)
        {
            var commands = container.FindCommands(phrase, pronunciation);
            if (commands.Count == 0)
                continue;

            foreach (var command in commands)
            {
                try
                {
                    if (!command.TryExecute())
                        LogError($"Command '{command.Id}' failed to execute. Check log for more information.");
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                }
            }
        }
    }
}