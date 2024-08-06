using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Commands;

public class CommandTrigger
{
    public string? Trigger;
    public string? Pronunciation;

    public bool IsCommand(string phrase)
    {
        return Trigger.ContainsOrFalse(phrase) || Pronunciation.ContainsOrFalse(phrase);
    }
}

public class Command
{
    public string Name = "Unnamed command";
    
    public List<string> Triggers = new(); // "Hey Windows"
    public ICommandExecutor? Executor; // Command action.. duh...
    public ICommandArgs? Arguments; // Command action.. duh...

    public bool IsActive = true;

    public bool TryExecute() => Executor?.TryExecute(Arguments ?? throw new ArgumentNullException($"Arguments were null for command '{Name}'.")) 
                                ?? throw new InvalidDataException("Command does not have an action associated with it.");

    public void AddTrigger(string trigger) => Triggers.Add(trigger);

    public void AddTriggers(params string[] triggers) => triggers.Loop(AddTrigger);
}