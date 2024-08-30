using System.Diagnostics;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace HeyWindows.Core.Commands.Executors;

public class InternetRequestCommandArgs : ICommandArgs
{
    [ArgumentField("URL", "Where to send the request to.", "https://example.com/resource")]
    public string Url;
    
    [ArgumentField("Method", "The type of request to send.", "GET")]
    public Method Method;
    
    [ArgumentField("Content Type", "The type of the body", "application/json...")]
    public string? ContentType;
    
    [ArgumentField("Body", "Text to add to the body.", "{ \"CoolVariable\": 123 }")]
    public string? Body;
    
    [ArgumentField("File", "A file to add to the body.", "C:\\File.json", StringInputType.File)]
    public string? File;
    
    public InternetRequestCommandArgs() { }
    
    [JsonConstructor]
    public InternetRequestCommandArgs(string Url, Method Method, string File)
    {
        this.Url = Url;
        this.Method = Method;
        this.File = File;
    }
}

public class InternetRequestExecutor : ICommandExecutor
{
    public ICommandArgs ArgumentHandler { get; } = new InternetRequestCommandArgs();
    
    public string Name => "InternetRequestExecutor";

    public bool CanExecute(ICommandArgs args) => true;
    
    public void Execute(ICommandArgs args)
    {
        try
        {
            var apiArgs = (InternetRequestCommandArgs)args;
            var client = new RestClient();
            var request = new RestRequest(apiArgs.Url, apiArgs.Method);

            request.AddBody(apiArgs.Body!, apiArgs.ContentType);
            client.Execute(request);
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
}