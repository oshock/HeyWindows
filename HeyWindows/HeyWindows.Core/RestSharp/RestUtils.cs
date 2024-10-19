using System.Text;
using Newtonsoft.Json;
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

public class RestBuilder
{
    public RestController Controller { get; }
    public RestRequest Request { get; }

    public RestBuilder(RestController controller, RestRequest request)
    {
        Controller = controller;
        Request = request;
    }

    public void AddHeaders(params (string name, string value)[] headers)
    {
        foreach (var (name, value) in headers)
        {
            Request.AddHeader(name, value);
        }
    }
}

public static class RestUtils
{
    public static RestController CreateController(this Uri uri) => new RestController(new RestClient(uri));

    
    /// <summary>
    /// Creates and sends a request
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="method"></param>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static RestResponse SendRequest(this RestController controller, Method method, string resource,
        params (string name, string value)[] headers)
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

    public static RestBuilder CreateRequest(this RestController controller, string resource)
    {
        LogInfo($"Creating request '{controller.Client.Options.BaseUrl + resource}'");
        var request = new RestRequest(resource);
        return new RestBuilder(controller, request);
    }

    public static RestBuilder WithHeaders(this RestBuilder builder, params (string name, string value)[] headers)
    {
        builder.AddHeaders(headers);
        return builder;
    }

    public static RestBuilder WithJsonBody(this RestBuilder builder, object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        builder.Request.AddJsonBody(content);
        return builder;
    }
    
    public static RestResponse Send(this RestBuilder builder, Method method)
    {
        return builder.Controller.Client.Execute(builder.Request, method);
    }
}