namespace HeyWindows.Core.Commands.Executors;

public interface ICommandArgs
{
    
}

public interface ICommandExecutor
{
    public string Name { get; protected set; }

    public bool TryExecute(ICommandArgs args)
    {
        if (!CanExecute(args))
            return false;

        Execute(args);
        return true;
    }
    
    public bool CanExecute(ICommandArgs args);
    
    public void Execute(ICommandArgs args);
}