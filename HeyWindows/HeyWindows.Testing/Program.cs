using HeyWindows.Core.Commands;
using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Logging;

Logger.StartLogger("test.log");
var commander = new Commander();
commander.Initialize();

var container = new CommandContainer("Main", new Command
    {
        Name = "Chrome",
        Triggers = new()
        {
            "Hey Windows open chrome"
        },
        Executor = new ExecutableExecutor
        {
            Name = "Chrome opener"
        },
        Arguments = new ExecutableCommandArgs
        {
            FilePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Elevated = true
        }
    },
    new Command
    {
        Name = "Notepad",
        Triggers = new()
        {
            "Hey Windows open notepad"
        },
        Executor = new ExecutableExecutor
        {
            Name = "Notepad opener"
        },
        Arguments = new ExecutableCommandArgs
        {
            FilePath =
                @"C:\Program Files\WindowsApps\Microsoft.WindowsNotepad_11.2405.13.0_x64__8wekyb3d8bbwe\Notepad\Notepad.exe",
            Elevated = true
        }
    });
commander.InitializeContainer(container);
commander.Activate();

while (true)
{
    Console.ReadLine();
}