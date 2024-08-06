using System.IO;
using System.Text.Json.Serialization;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Utils;

namespace HeyWindows.App.Configs;

public class Config
{
    public List<Command>? Commands;
}

public static class ConfigReader
{
    public static string DIRECTORYPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HeyWindows");
    public static string FILENAME = "Config.json";
    public static string FILEPATH = Path.Combine(DIRECTORYPATH, FILENAME);

    public static Config? Config;

    public static Config Reload()
    {
        if (!File.Exists(FILEPATH))
            Save();

        var text = File.ReadAllText(FILEPATH);
        return Config = text.FromJson<Config>();
    }

    public static void Save()
    {
        if (!Directory.Exists(DIRECTORYPATH))
            Directory.CreateDirectory(DIRECTORYPATH);

        var data = Config ?? new Config();
        File.WriteAllText(FILEPATH, data.ToJsonString());
    }
}