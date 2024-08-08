namespace HeyWindows.Core.Commands.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ArgumentFieldAttribute : Attribute
{
    public string DisplayName { get; }
    public string? Description { get; }
    public string? Placeholder { get; }

    public ArgumentFieldAttribute(string DisplayName, string? Description = null, string? Placeholder = null)
    {
        this.DisplayName = DisplayName;
        this.Description = Description;
        this.Placeholder = Placeholder;
    }
}