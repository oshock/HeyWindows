﻿using System.Diagnostics;
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
    
    [ArgumentField("Body", "Text to add to the body (Leave empty if you would like to use a file instead).", "{ \"CoolVariable\": 123 }")]
    public string? Body;
    
    [ArgumentField("File", "A file to add to the body.", "C:\\File.json", StringInputType.File)]
    public string? File;
    
    [ArgumentField("Response Output", "The file where we'll write the response (Leave empty if not needed).", "C:\\Response.json", StringInputType.File)]
    public string? Output;
    
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
            LogInfo($"Initialized request: '{apiArgs.Method} :: {apiArgs.Url}'.");
            
            LogInfo("Adding body...");
            if (string.IsNullOrEmpty(apiArgs.Body)) 
                request.AddBody(apiArgs.Body!, apiArgs.ContentType);
            else if (!string.IsNullOrEmpty(apiArgs.File))
            {
                var fileContent = File.ReadAllBytes(apiArgs.File);
                request.AddBody(fileContent, apiArgs.ContentType);    
            }
            
            var response = client.Execute(request);
            LogInfo($"Executed request: '{response.Content}'.");
            
            if (string.IsNullOrEmpty(apiArgs.Output))
                return;
            
            LogInfo($"Writing response to: '{apiArgs.Output}'.");
            File.WriteAllText(apiArgs.Output!, response.Content);
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
}