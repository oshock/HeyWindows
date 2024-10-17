using HeyWindows.Core.Commands;
using HeyWindows.Core.Commands.Executors.Spotify;
using HeyWindows.Core.Logging;

Logger.StartLogger("test.log");
/*var commander = new Commander();
commander.Initialize();

var container = new CommandContainer("Main", new Command
    {
    });
commander.InitializeContainer(container);
commander.Activate();*/

var spotify = new SpotifyController();
spotify.Initialize("78bd25d7b17f40fbafba00fe5d63af73", "6b18e6fa86da4287945b54323c5debba", "http://localhost:5543");
Console.WriteLine(spotify.StartAuthorization());

while (!spotify.IsReady())
{
    Thread.Sleep(100);
}

spotify.ResumeMusic();