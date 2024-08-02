using System.Diagnostics;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Commands.Variables;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Commands.Executors;

public class ExecutableCommandArgs : ICommandArgs
{
    [ArgumentField("Executable File Location", "The path of the .exe file that will be executed.")]
    public string FilePath;
    
    [ArgumentField("Arguments", "The arguments that the executable will startup with.")]
    public string Args;
    
    [ArgumentField("Elevated Permissions", "Should the process start with administrator permissions?")]
    public bool Elevated;
    
    [ArgumentField("Wait For Exit", "Should we wait for the process to exit before executing more commands?")]
    public bool WaitForExit;

    public static ICommandArgs Parse(string[] words)
    {
        var instance = new ExecutableCommandArgs();

        var withIndex = words.FindIndex("with");
        var trigger = words.MergeStringArray();
        if (withIndex > 0)
            trigger = words.SelectRange(0, withIndex).MergeStringArray();

        if (!SavedDataReader.TryGetAll(trigger, out var variables))
            throw new KeyNotFoundException($"Could not find variable with trigger '{trigger}'.");
        
        // TODO ask for clarifcation
        var variable = variables.First();
        instance.FilePath =
            variable.Type == VariableType.String
                ? variable.Value?.ToString()!
                : throw new InvalidDataException(
                    $"Variable '{variable.Name}' expected to be {VariableType.String}, got '{variable.Type}'. \nValue: '{variable.Value}'");

        instance.Elevated = words.ContainsAnyToLower("elevated", "admin"); 
        
        return instance;
    }
}

public class ExecutableExecutor : ICommandExecutor
{
    public ICommandArgs ArgumentHandler { get; } = new ExecutableCommandArgs();
    
    public string Name { get; set; } = "Process Executor";

    public bool CanExecute(ICommandArgs args)
    {
        var exeArgs = args.Cast<ExecutableCommandArgs>()!;
        return File.Exists(exeArgs.FilePath);
    }
    
    public void Execute(ICommandArgs args)
    {
        try
        {
            var exeArgs = args.Cast<ExecutableCommandArgs>()!;
            var startInfo = new ProcessStartInfo
            {
                FileName = exeArgs.FilePath,
                Arguments = exeArgs.Args
            };

            if (exeArgs.Elevated)
            {

            }

            var proc = Process.Start(startInfo)!;
            if (!exeArgs.WaitForExit)
                return;

            proc.WaitForExit();
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
}