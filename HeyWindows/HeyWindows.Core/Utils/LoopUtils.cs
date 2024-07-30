namespace HeyWindows.Core.Utils;

public static class LoopUtils
{
    public static void Loop<T>(this ICollection<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }
}