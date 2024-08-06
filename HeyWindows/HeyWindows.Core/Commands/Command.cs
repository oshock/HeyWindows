using System.Reflection;
using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Utils;
using Newtonsoft.Json;

namespace HeyWindows.Core.Commands;

public class CommandTrigger
{
    public string? Trigger;
    public string? Pronunciation;

    public CommandTrigger(string? trigger, string? pronunciation = null)
    {
        Trigger = trigger;
        Pronunciation = pronunciation;
    }

    public bool IsCommand(string? phrase)
    {
        if (phrase is null)
            return false;
        return Trigger.ContainsOrFalse(phrase) || Pronunciation.ContainsOrFalse(phrase);
    }
}

public class Command
{
    public string Name = "Unnamed command";
    
    public List<CommandTrigger> Triggers = new(); // "Hey Windows"
    
    [JsonIgnore]
    private ICommandExecutor? _executor; // Command action.. duh...

    public string Executor
    {
        get
        {
            return Commander.ExecutorByName.FirstOrDefault(x => x.Value.GetType() == _executor?.GetType()).Key;
        }
        init
        {
            if (!Commander.ExecutorByName.TryGetValue(value, out var executor))
                throw new KeyNotFoundException(
                    $"Could not find executor named '{value}'. \nAvailable executors: \n{Commander.ExecutorByName.Keys.MergeStringArray("\n")}");

            _executor = executor;
        }
    }
    
    public ICommandArgs? Arguments; // Command action.. duh...
    
    public bool IsActive = true;

    public bool TryExecute() => _executor?.TryExecute(Arguments ?? throw new ArgumentNullException($"Arguments were null for command '{Name}'.")) 
                                ?? throw new InvalidDataException("Command does not have an action associated with it.");

    public void AddTrigger(CommandTrigger trigger) => Triggers.Add(trigger);

    public bool IsCommand(string? trigger) => Triggers.Any(x => x.IsCommand(trigger));
}