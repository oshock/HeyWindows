using Newtonsoft.Json;

namespace HeyWindows.Core.Commands.Executors.Spotify;

public class SpotifySearchResponse
{
    [JsonProperty("tracks")]
    public SpotifyTrackResult Tracks { get; set; }
}

public class SpotifyTrackResult
{
    [JsonProperty("items")]
    public List<SpotifyTrack> Items { get; set; }
}

public class SpotifyTrack
{
    [JsonProperty("uri")]
    public string Uri { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}

public class SpotifyDevicesResponse
{
    [JsonProperty("devices")]
    public List<SpotifyDevice> Devices { get; set; }
}

public class SpotifyDevice
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("is_active")]
    public bool IsActive { get; set; }

    [JsonProperty("is_restricted")]
    public bool IsRestricted { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}