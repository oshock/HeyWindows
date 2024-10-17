using System.Net;
using System.Text;
using HeyWindows.Core.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace HeyWindows.Core.Commands.Executors.Spotify;

// Token response class to deserialize JSON response
public class SpotifyTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
    
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}

public class SpotifyController
{
    private const string SPOTIFY_API_URL = "https://api.spotify.com/v1/me/player";
    private string? _accessToken;
    private RestController controller;
    
    public SpotifyController()
    {
        controller = new Uri(SPOTIFY_API_URL).CreateController();
    }

    public bool IsReady() => _accessToken is not null;

    /// <summary>
    /// Must be called so that this controller may interface with your Spotify Application.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
    /// <param name="callbackUrl"></param>
    public void Initialize(string clientId, string clientSecret, string callbackUrl)
    {
        SetClientId(clientId);
        SetClientSecret(clientSecret);
        SetCallbackUrl(callbackUrl);
    }
    
    private string? _clientId;
    public void SetClientId(string id) => _clientId = id;
    
    private string? _clientSecret;
    public void SetClientSecret(string secret) => _clientSecret = secret;

    private string? _callbackUrl;
    public void SetCallbackUrl(string url) => _callbackUrl = url;

    private async Task SetAccessToken(string code)
    {
        var client = new HttpClient();
        var requestBody = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", code),
            new("redirect_uri", _callbackUrl),
            new("client_id", _clientId),
            new("client_secret", _clientSecret)
        };

        var requestContent = new FormUrlEncodedContent(requestBody);

        var response = await client.PostAsync("https://accounts.spotify.com/api/token", requestContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        var tokenResponse = JsonConvert.DeserializeObject<SpotifyTokenResponse>(responseContent);
        _accessToken = tokenResponse.AccessToken;
    }
    
    /// <summary>
    /// Creates an authorization url and starts listening for a callback. When callback has been received the access token will be registered to this controller.
    /// </summary>
    /// <returns>Authorization Url</returns>
    public string? StartAuthorization()
    {
        if (string.IsNullOrEmpty(_clientId))
        {
            LogError("Cannot create authorization url because client id is not present.");
            return null;
        }
        
        if (string.IsNullOrEmpty(_clientSecret))
        {
            LogError("Cannot create authorization url because client secret is not present.");
            return null;
        }
        
        if (string.IsNullOrEmpty(_callbackUrl))
        {
            LogError("Cannot create authorization url because callback url is not present.");
            return null;
        }

        var scope = "user-modify-playback-state user-read-playback-state";
        var state = Guid.NewGuid().ToString();

        var listener = new HttpListener();
        listener.Prefixes.Add(_callbackUrl.EndsWith("/") ? _callbackUrl : _callbackUrl + "/");

        // Start the listener
        listener.Start();
        LogInfo($"Server is listening on '{_callbackUrl}'");

        new Task(() =>
        {
            var context = listener.GetContext();
            var request = context.Request;
            var response = context.Response;
            
            SetAccessToken(request.QueryString["code"]).GetAwaiter().GetResult(); 

            response.OutputStream.Write("Authorized."u8);
            response.OutputStream.Close();
            response.Close();
            listener.Close();
            
            LogInfo($"'{_callbackUrl}' was consumed.");
        }).Start();
        
        var url =
            $"https://accounts.spotify.com/authorize?client_id={_clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(_callbackUrl)}&scope={Uri.EscapeDataString(scope)}&state={state}";

        return url;
    }

    public void PauseMusic()
    {
        try
        {
            var response = controller.SendRequest(Method.Put, "/pause",
                ("Authorization", $"Bearer {_accessToken}")
                );

            if (response.IsSuccessful)
            {
                LogInfo("Paused music.");
            }
            else
            {
                LogError("Failed to pause music. \n" + response.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
    
    public void ResumeMusic()
    {
        try
        {
            var response = controller.SendRequest(Method.Put, "/play",
                ("Authorization", $"Bearer {_accessToken}")
            );

            if (response.IsSuccessful)
            {
                LogInfo("Resumed music.");
            }
            else
            {
                LogError("Failed to resume music. \n" + response.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
}