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
    
    public CommandContainer? CommandContainer;
    
    public void InitializeContainer(CommandContainer container, bool isMainCommands = false)
    {
        CommandContainer = container;
        
        if (_listener is null)
            throw new InvalidOperationException("Cannot add commands without a valid listener.");

        InitializeGrammars();
    }

    public void InitializeGrammars()
    {
        if (CommandContainer!.Commands.Count > 0)
            _listener?.Grammars.Add(CommandContainer.Commands.BuildGrammarFromCommands());

        _listener?.Initialize(); // Consume added grammars
    }

    public void InitializeCommand(Command command)
    {
        CommandContainer!.Commands.Add(command);
        InitializeGrammars();
    }
    
    public void DeinitializeCommand(Command command)
    {
        CommandContainer!.Commands.Remove(command);
        InitializeGrammars();
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
        if (CommandContainer is null)
            throw new NullReferenceException("Cannot execute commands without a valid command container.");
        
            var commands = CommandContainer.FindCommands(phrase, pronunciation);
            if (commands.Count == 0)
            {
                LogVerbose($"Phrase '{phrase}' and '{pronunciation}' were not present in any active commands.");
            }

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