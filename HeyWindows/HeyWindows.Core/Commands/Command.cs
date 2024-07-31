using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Commands;

public class CommandContext
{
    public Command Current; // Could be "Open ..." inside of parent command "Hey Windows" 
    public bool ActionPreformed = false;
}

public class Command
{
    public string Name = "Unnamed command";
    
    public List<string> Triggers = new(); // "Hey Windows"
    public Command? Group; // Parent trigger/command
    public List<Command> SubCommands = new(); // "Hey Windows" --> "Open ..."
    public Action? Action; // Command action.. duh...

    public bool IsActive = true;

    public Command? FindCommand(string subCommand) => SubCommands.FirstOrDefault(x => x.Triggers.Contains(subCommand));

    public void AddSubCommand(Command command) => SubCommands.Contains(command).DoIf(() => SubCommands.Add(command));

    public void AddTrigger(string trigger) => Triggers.Add(trigger);

    public void AddTriggers(params string[] triggers) => triggers.Loop(AddTrigger);
}