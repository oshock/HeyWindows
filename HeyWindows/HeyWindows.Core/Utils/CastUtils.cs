using System.Reflection;
using HeyWindows.Core.Commands.Attributes;

namespace HeyWindows.Core.Utils;

public static class CastUtils
{
    public static T? Cast<T>(this object obj) where T : class
    {
        return obj as T;
    }
    
    public static ArgumentFieldAttribute GetArgumentAttribute(this FieldInfo field) => (ArgumentFieldAttribute)field.GetCustomAttributes(typeof(ArgumentFieldAttribute), false).First();
}