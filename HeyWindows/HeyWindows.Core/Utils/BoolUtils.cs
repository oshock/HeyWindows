namespace HeyWindows.Core.Utils;

public static class BoolUtils
{
    public static void DoIf(this bool truthfulness, Action action)
    {
        if (!truthfulness)
            return;

        action();
    }

    public static bool ContainsOrFalse(this string? s, string str)
    {
        return s is not null && s.Contains(str);
    }
}