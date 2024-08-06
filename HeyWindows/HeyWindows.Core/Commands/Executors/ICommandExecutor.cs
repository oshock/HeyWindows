namespace HeyWindows.Core.Commands.Executors;

public interface ICommandArgs
{
    
}

public interface ICommandExecutor
{
    public ICommandArgs ArgumentHandler { get; }
    
    public string Name { get; }
    
    public bool TryExecute(ICommandArgs args)
    {
        LogInfo($"Attempting to execute command: '{Name}'.");
        if (!CanExecute(args))
        {
            LogWarn($"Could not execute command: '{Name}'.");
            return false;
        }

        Execute(args);
        LogInfo($"Executed command: '{Name}'.");
        return true;
    }
    
    public bool CanExecute(ICommandArgs args);
    
    public void Execute(ICommandArgs args);
}