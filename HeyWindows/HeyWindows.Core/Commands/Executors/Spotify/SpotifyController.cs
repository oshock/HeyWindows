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
    private const string SPOTIFY_API_URL = "https://api.spotify.com/v1";
    private const string AUTH_SCOPE = "user-modify-playback-state user-read-playback-state";
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
            var response = controller.SendRequest(Method.Put, "/me/player/pause",
                ("Authorization", $"Bearer {_accessToken}")
                );

            if (response.IsSuccessful)
            {
                LogInfo("Paused music.");
            }
            else
            {
                LogError("Failed to pause music. \n" + response.Content);
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
            var response = controller.SendRequest(Method.Put, "/me/player/play",
                ("Authorization", $"Bearer {_accessToken}")
            );

            if (response.IsSuccessful)
            {
                LogInfo("Resumed music.");
            }
            else
            {
                LogError("Failed to resume music. \n" + response.Content);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
    
    public void AddSongToQueue(string query, string deviceId)
    {
        try
        {
            var response = controller.SendRequest(Method.Get, $"/search?q={query}&type=track&limit=10",
                ("Authorization", $"Bearer {_accessToken}")
            );

            if (response.IsSuccessful)
            {
                
                var json = JsonConvert.DeserializeObject<SpotifySearchResponse>(response.Content);
                if (json.Tracks.Items.Count == 0)
                {
                    LogInfo("No tracks found.");
                    return;
                }
                
                LogInfo("Found track(s).");

                var track = json.Tracks.Items[0];
                var playResponse = controller.SendRequest(Method.Post,
                    $"/me/player/queue?uri={track.Uri}&device_id={deviceId}",
                    ("Authorization", $"Bearer {_accessToken}"));

                if (playResponse.IsSuccessStatusCode)
                {
                    LogInfo($"Added song '{track.Name}' to queue.");
                }
                else
                {
                    LogError("Failed to add song to queue. \n" + playResponse.Content);
                }
            }
            else
            {
                LogError("Failed to find tracks. \n" + response.Content);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
    
    public void SearchAndPlaySong(string query, string deviceId)
    {
        try
        {
            AddSongToQueue(query, deviceId);
            
            var response = controller.SendRequest(Method.Post, $"/me/player/next?device_id={deviceId}",
                ("Authorization", $"Bearer {_accessToken}")
            );

            if (response.IsSuccessful)
            {
                LogInfo("Playing song.");
            }
            else
            {
                LogError("Failed to play song. \n" + response.Content);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
    }
    
    public List<SpotifyDevice>? GetDevices()
    {
        try
        {
            var response = controller.SendRequest(Method.Get, "/me/player/devices",
                ("Authorization", $"Bearer {_accessToken}")
            );

            if (response.IsSuccessful)
            {
                LogInfo("Successfully retrieved device list.");
                
                var json = JsonConvert.DeserializeObject<SpotifyDevicesResponse>(response.Content);
                return json.Devices;
            }

            LogError("Failed to find devices. \n" + response.Content);
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
        }
        
        return null;
    }
}