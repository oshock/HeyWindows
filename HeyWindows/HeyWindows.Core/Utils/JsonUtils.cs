using Newtonsoft.Json;

namespace HeyWindows.Core.Utils;

public static class JsonUtils
{
    public static string ToJsonString(this object obj, Formatting formatting = Formatting.Indented) => JsonConvert.SerializeObject(obj, 
        new JsonSerializerSettings
        {
           Formatting = formatting,
           TypeNameHandling = TypeNameHandling.Auto
        });

    public static T FromJson<T>(this string obj) => JsonConvert.DeserializeObject<T>(obj, new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    });
}