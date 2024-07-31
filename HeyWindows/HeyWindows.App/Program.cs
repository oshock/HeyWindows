using System.Speech.Recognition;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Grammars;
using HeyWindows.Core.Listeners;

var commander = new Commander();
commander.Initialize();

var container = new CommandContainer("Main", new Command
{
    Name = "Open",
    Triggers = new List<string>()
    {
        "Open"
    },
    SubCommands = new List<Command>
    {
        new()
        {
            Name = "Chrome",
            Triggers = new List<string>()
            {
                "Chrome"
            },
            Action = () => Console.WriteLine("\nOpening chrome!!!!!!!!!\n")
        },
        new()
        {
            Name = "Fortnite",
            Triggers = new List<string>()
            {
                "Fortnite"
            },
            Action = () => Console.WriteLine("\nOpening Fortnite!!!!!!!!!\n")
        },
        new()
        {
            Name = "Edge",
            Triggers = new List<string>()
            {
                "Edge"
            },
            Action = () => Console.WriteLine("\nOpening Edge!!!!!!!!!\n")
        }
    }
});
commander.InitializeContainer(container);
commander.Activate();

/*
var command = GrammarCreator.Create();
command.IncorporatePhrase("Hey Windows");
command.AppendBuilder(GrammarCreator.CreateChoice("Open Google", "Open Chrome"));

listener.Grammars.Add(command.Builder);
listener.Initialize();
listener.Listen();
*/

while (true)
{
    Console.ReadLine();
}