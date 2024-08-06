using System.Text.Json.Serialization;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Commands.Variables;

public enum VariableType
{
    String,
    Int32,
    Int64,
    Float,
    Boolean
}

public class SavedVariable
{
    [JsonPropertyName("name")]
    public string? Name;
    
    [JsonPropertyName("type")]
    public VariableType Type;
    
    [JsonPropertyName("value")]
    public object? Value;
}

public class SavedVariableContainer
{
    public List<SavedVariable>? Variables = new();
}

public static class SavedDataReader
{
    public static string DATA_DIRECTORYPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HeyWindows");
    public static string DATA_FILENAME = "Variables.json";
    public static string DATA_FILEPATH = Path.Combine(DATA_DIRECTORYPATH, DATA_FILENAME);

    private static SavedVariableContainer? _cached = null;

    public static SavedVariableContainer Reload()
    {
        if (!File.Exists(DATA_FILEPATH))
            Save();

        var text = File.ReadAllText(DATA_FILEPATH);
        return _cached = text.FromJson<SavedVariableContainer>();
    }

    public static void Save()
    {
        if (!Directory.Exists(DATA_DIRECTORYPATH))
            Directory.CreateDirectory(DATA_DIRECTORYPATH);

        var data = _cached ?? new SavedVariableContainer();
        File.WriteAllText(DATA_FILEPATH, data.ToJsonString());
    }
    
    public static bool TryGet(string name, out SavedVariable? variable)
    {
        variable = (_cached?.Variables ?? throw new InvalidDataException($"'{DATA_FILEPATH}' has not been read."))
            .FirstOrDefault(x => x.Name == name);
        return !variable?.Equals(default) ?? false;
    }
}