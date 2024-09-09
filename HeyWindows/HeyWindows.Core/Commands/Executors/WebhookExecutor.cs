using System.Diagnostics;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace HeyWindows.Core.Commands.Executors;

public class WebhookCommandArgs : ICommandArgs
{
    [ArgumentField("URL", "The webhook's url.", "https://example.com/resource")]
    public string Url;
    
    public WebhookCommandArgs() { }
    
    [JsonConstructor]
    public WebhookCommandArgs(string Url, Method Method, string File)
    {
        this.Url = Url;
    }
}

public class WebhookExecutor : ICommandExecutor
{
    public ICommandArgs ArgumentHandler { get; } = new WebhookCommandArgs();
    
    public string Name => "WebhookExecutor";

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