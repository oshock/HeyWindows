namespace HeyWindows.Core.Utils;

public static class CastUtils
{
    public static T? Cast<T>(this object obj) where T : class
    {
        return obj as T;
    }
}