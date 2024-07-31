namespace HeyWindows.Core.Commands.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ArgumentFieldAttribute : Attribute
{
    public string DisplayName { get; }
    public string Description { get; }

    public ArgumentFieldAttribute(string DisplayName, string Description)
    {
        this.DisplayName = DisplayName;
        this.Description = Description;
    }
}