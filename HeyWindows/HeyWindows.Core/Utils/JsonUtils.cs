using Newtonsoft.Json;

namespace HeyWindows.Core.Utils;

public static class JsonUtils
{
    public static string ToJsonString(this object obj, Formatting formatting = Formatting.Indented) => JsonConvert.SerializeObject(obj, formatting);
    public static T FromJson<T>(this string obj) => JsonConvert.DeserializeObject<T>(obj);
}