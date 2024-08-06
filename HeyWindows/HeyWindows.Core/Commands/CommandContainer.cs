using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Commands;

public class CommandContainer
{
    public readonly string Name;
    public readonly List<Command> Commands = new();

    public CommandContainer(string name, List<Command> commands)
    {
        Name = name;
        Commands = commands;
    }
    
    public void EnableCommand(Command command)
    {
        Commands.DoFor(x => x == command, 
            x => x.IsActive = true, 
            onlyFirst: true);
    }

    public void EnableAll() => Commands.Loop(x => x.IsActive = true);
    
    public void DisableCommand(Command command)
    {
        Commands.DoFor(x => x == command, 
            x => x.IsActive = false, 
            onlyFirst: true);
    }
    
    public void DisableAll() => Commands.Loop(x => x.IsActive = false);

    public CommandContainer(string name, params Command[] commands)
    {
        Name = name;
        Commands.AddRange(commands);
    }

    public List<Command> FindCommands(string trigger, string? pronunciation = null) => Commands.Where(x => x.IsCommand(trigger) || x.IsCommand(pronunciation)).ToList();
}