using System.Speech.Recognition;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Commands.Executors.Spotify;
using HeyWindows.Core.Listeners;
using HeyWindows.Core.Logging;

Logger.StartLogger("test.log");

var spotify = new SpotifyController();
spotify.Initialize("client_id", "client_secret", "http://localhost:5543");
Console.WriteLine(spotify.StartAuthorization());

while (!spotify.IsReady())
{
    Thread.Sleep(100);
}

var listener = new Listener();
listener.AddWord("windows add *");
listener.AddWord("windows pause music");
listener.AddWord("windows unpause music");
listener.AddWord("windows resume music");
listener.AddWord("windows skip song");
listener.Initialize();

var devices = spotify.GetDevices();
var deviceId = devices.First(x => x.IsActive).Id;

listener.ListenAsync(e =>
{
    var text = e.Result.Text.ToLower();
    
    /*if (text.Contains("windows add"))
    {
        spotify.AddSongToQueue(e.Result.Text.Replace("windows add", string.Empty), deviceId);
    }
    else */if (text.Contains("windows skip song"))
    {
        spotify.Skip(deviceId);
    }
    else if (text.Contains("windows pause music"))
    {
        spotify.PauseMusic();
    }
    else if (text.Contains("windows unpause music") || text.Contains("windows resume music"))
    {
        spotify.ResumeMusic();
    }
});

while (true)
{
    Thread.Sleep(1000);
}
    
    