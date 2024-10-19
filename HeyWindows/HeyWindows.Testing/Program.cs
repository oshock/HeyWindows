using HeyWindows.Core.Commands;
using HeyWindows.Core.Commands.Executors.Spotify;
using HeyWindows.Core.Listeners;
using HeyWindows.Core.Logging;

Logger.StartLogger("test.log");

var spotify = new SpotifyController();
spotify.Initialize("78bd25d7b17f40fbafba00fe5d63af73", "6b18e6fa86da4287945b54323c5debba", "http://localhost:5543");
Console.WriteLine(spotify.StartAuthorization());

while (!spotify.IsReady())
{
    Thread.Sleep(100);
}

var listener = new Listener();
listener.Initialize();
listener.ListenSingleAsync(e =>
{
    var devices = spotify.GetDevices();
    spotify.SearchAndPlaySong(e.Result.Text, devices.First(x => x.IsActive).Id);
});

while (true)
{
    Thread.Sleep(1000);
}