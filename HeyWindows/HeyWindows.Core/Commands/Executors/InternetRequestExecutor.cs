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
    
    [ArgumentField("File", "A file to add to the body.", "C:\\File.json", StringInputType.File)]
    public string File;
    
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
            
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
}