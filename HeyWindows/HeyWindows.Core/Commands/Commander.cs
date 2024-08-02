using System.Data;
using System.Speech.Recognition;
using HeyWindows.Core.Grammars;
using HeyWindows.Core.Listeners;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Commands;

public class Commander
{
    private readonly List<CommandContainer> _containers = new();

    public static Command RootCommand = new()
    {
        Name = "Activation",
        Triggers = { "Hey Windows", "Windows" }
    };
    
    public void InitializeContainer(CommandContainer container)
    {
        if (_listener is null)
            throw new InvalidOperationException("Cannot add commands without a valid listener.");

        _containers.Add(container);

        var root = RootCommand;
        root.SubCommands = container.Commands;
        
        var builder = root.BuildGrammarFromCommand();
        _listener?.Grammars.Add(builder);

        _listener.Initialize(); // Consume added grammars
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
        Command? current; 
        var words = phrase.Split(' ');
        int index;

        if (!IsCommand(RootCommand, words, out index))
            return;

        var afterRootIndex = index;
        
        foreach (var container in _containers)
        {
            foreach (var command in container.Commands)
            {
                if (!IsCommand(command, words, out var finishIndex, index))
                    continue;

                current = command;
                index = finishIndex;
                
                while (index < words.Length && current.SubCommands.Count > 0)
                {
                    foreach (var cmd in current.SubCommands)
                    {
                        if (!IsCommand(cmd, words, out var subFinishIndex, index))
                            continue;

                        current = cmd;
                        index = subFinishIndex;
                        break;
                    }
                    
                    if (current is null)
                        continue;

                    if (current.Executor is null)
                        throw new InvalidExpressionException($"'{current.Name}' does not have an executor to execute.");

                    // TODO if (current.Executor.TryExecute())
                    {
                        
                    }
                    return;
                }
            }
        }
    }

    private bool IsCommand(Command command, string[] words, out int index, int startIndex = 0)
    {
        index = startIndex;
        foreach (var trigger in command.Triggers)
        {
            if (string.IsNullOrEmpty(trigger))
                throw new InvalidDataException("Trigger cannot be null or empty.");
                    
            var text = new string(trigger);

            while (!string.IsNullOrEmpty(text))
            {
                if (!text.StartsWith(words[index]))
                    break;

                text = text.SubstringAfter(" ");
                index++;
            }

            if (!string.IsNullOrEmpty(text))
                continue;

            return true;
        }

        return false;
    }
}