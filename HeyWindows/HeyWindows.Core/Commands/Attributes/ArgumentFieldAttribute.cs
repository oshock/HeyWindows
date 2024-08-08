namespace HeyWindows.Core.Commands.Attributes;

public enum StringInputType
{
    Regular,
    File,
    Directory
}

[AttributeUsage(AttributeTargets.Field)]
public class ArgumentFieldAttribute : Attribute
{
    public string DisplayName { get; }
    public string? Description { get; }
    public string? Placeholder { get; }
    public StringInputType Type { get; }

    public ArgumentFieldAttribute(string DisplayName, string? Description = null, string? Placeholder = null, StringInputType Type = StringInputType.Regular)
    {
        this.DisplayName = DisplayName;
        this.Description = Description;
        this.Placeholder = Placeholder;
        this.Type = Type;
    }
}