using HeyWindows.Core.Commands;
using HeyWindows.Core.Logging;

Logger.StartLogger("test.log");
var commander = new Commander();
commander.Initialize();

var container = new CommandContainer("Main", new Command
    {
    });
commander.InitializeContainer(container);
commander.Activate();

while (true)
{
    Console.ReadLine();
}