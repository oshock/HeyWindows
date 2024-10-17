using RestSharp;

namespace HeyWindows.Core.RestSharp;

public class RestController
{
    public RestClient Client { get; }

    public RestController(RestClient client)
    {
        Client = client;
    }
}

public static class RestUtils
{
    public static RestController CreateController(this Uri uri) => new RestController(new RestClient(uri));

    public static RestResponse SendRequest(this RestController controller, Method method, string resource, params (string name, string value)[] headers)
    {
        LogInfo($"Creating request '{controller.Client.Options.BaseUrl + resource}'");
        var request = new RestRequest(resource);
        request.Method = method;
        
        foreach (var (name, value) in headers)
        {
            request.AddHeader(name, value);
        }
        
        return controller.Client.Execute(request);
    }
}