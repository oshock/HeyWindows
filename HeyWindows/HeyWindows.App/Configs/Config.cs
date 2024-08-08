using System.IO;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Utils;

namespace HeyWindows.App.Configs;

public class Config
{
    public List<Command> Commands = new();
}

public static class ConfigSystem
{
    public static string DATA_DIRECTORYPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HeyWindows");
    public static string CONFIG_FILENAME = "Config.json";
    public static string CONFIG_FILEPATH = Path.Combine(DATA_DIRECTORYPATH, CONFIG_FILENAME);

    public static string LOG_FILENAME = $"HeyWindows-{DateTime.UtcNow.ToString().Replace("/", "-")}.log";
    public static string LOG_FILEPATH = Path.Combine(DATA_DIRECTORYPATH, LOG_FILENAME);

    public static Config? ConfigData;

    public static Config ReloadConfig()
    {
        if (!File.Exists(CONFIG_FILEPATH))
            SaveConfig();

        var text = File.ReadAllText(CONFIG_FILEPATH);
        return ConfigData = text.FromJson<Config>();
    }

    public static void SaveConfig()
    {
        if (!Directory.Exists(DATA_DIRECTORYPATH))
            Directory.CreateDirectory(DATA_DIRECTORYPATH);

        var data = ConfigData ?? new Config();
        File.WriteAllText(CONFIG_FILEPATH, data.ToJsonString());
    }
}