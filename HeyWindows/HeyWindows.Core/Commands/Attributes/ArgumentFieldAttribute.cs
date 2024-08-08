namespace HeyWindows.Core.Commands.Attributes;

public enum InputType
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
    public InputType Type { get; }

    public ArgumentFieldAttribute(string DisplayName, string? Description = null, string? Placeholder = null, InputType Type = InputType.Regular)
    {
        this.DisplayName = DisplayName;
        this.Description = Description;
        this.Placeholder = Placeholder;
        this.Type = Type;
    }
}