using System.Diagnostics;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Utils;
using Newtonsoft.Json;

namespace HeyWindows.Core.Commands.Executors;

public class ExecutableCommandArgs : ICommandArgs
{
    [ArgumentField("Executable File Location", "The path of the .exe file that will be executed.", "C:\\Folder\\File.exe...", InputType.File)]
    public string FilePath;
    
    [ArgumentField("Arguments", "The arguments that the executable will startup with.")]
    public string Args;
    
    [ArgumentField("Elevated Permissions", "Should the process start with administrator permissions?")]
    public bool Elevated;
    
    [ArgumentField("Wait For Exit", "Should we wait for the process to exit before executing more commands?")]
    public bool WaitForExit;

    public ExecutableCommandArgs() { }
    
    [JsonConstructor]
    public ExecutableCommandArgs(string FilePath, string Args, bool Elevated, bool WaitForExit)
    {
        this.FilePath = FilePath;
        this.Args = Args;
        this.Elevated = Elevated;
        this.WaitForExit = WaitForExit;
    }
}

public class ExecutableExecutor : ICommandExecutor
{
    public ICommandArgs ArgumentHandler { get; } = new ExecutableCommandArgs();
    
    public string Name => "ExecutableExecutor";

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
                startInfo.Verb = "runas";
                startInfo.UseShellExecute = true;
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