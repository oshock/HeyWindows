namespace HeyWindows.Core.Utils;

public static class ListUtils
{
    public static void DoFor<T>(this IEnumerable<T> list, Predicate<T> predicate, Action<T> action, bool onlyFirst = false)
    {
        foreach (var elm in list)
        {
            if (!predicate.Invoke(elm))
                continue;
            
            action(elm);
            if (onlyFirst)
                return;
        }
    }
    
}