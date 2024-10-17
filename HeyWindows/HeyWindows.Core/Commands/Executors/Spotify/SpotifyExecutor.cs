using System.Diagnostics;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace HeyWindows.Core.Commands.Executors.Spotify;
 
public class SpotifyCommandArgs : ICommandArgs
{
    [ArgumentField("URL", "The webhook's url.", "https://example.com/resource")]
    public string Url;
    
    public SpotifyCommandArgs() { }
    
    [JsonConstructor]
    public SpotifyCommandArgs(string Url, Method Method, string File)
    {
        this.Url = Url;
    }
}

public class SpotifyExecutor : ICommandExecutor
{
    public ICommandArgs ArgumentHandler { get; } = new SpotifyCommandArgs();
    
    public string Name => "SpotifyExecutor";

    public bool CanExecute(ICommandArgs args) => true;
    
    public void Execute(ICommandArgs args)
    {
        try
        {
            var apiArgs = (WebhookCommandArgs)args;
           
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
}