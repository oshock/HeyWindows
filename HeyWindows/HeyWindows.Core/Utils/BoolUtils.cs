namespace HeyWindows.Core.Utils;

public static class BoolUtils
{
    public static void DoIf(this bool truthfulness, Action action)
    {
        if (!truthfulness)
            return;

        action();
    }
}