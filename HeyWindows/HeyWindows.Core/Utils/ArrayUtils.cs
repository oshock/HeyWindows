namespace HeyWindows.Core.Utils;

public static class ArrayUtils
{
    public static int FindIndex(this object[] objs, object[] objectsToFind)
    {
        var found = false;
        var index = 0;
        var remaining = objectsToFind.Length;
        
        for (int i = 0; i < objs.Length && remaining > 0; i++)
        {
            found = objs[i].Equals(objectsToFind[objectsToFind.Length - remaining]);
            remaining--;
            index = i;
        }

        if (found && remaining > 0)
            return index;
        return -1;
    }

    public static int FindIndex(this object[] objs, object objectToFind) =>
        FindIndex(objs, new[] { objectToFind });

    public static T[] SelectRange<T>(this T[] array, int from, int to)
    {
        var block = new T[to - from + 1];
        Buffer.BlockCopy(array, from, block, 0, block.Length);
        return block;
    }

    public static string MergeStringArray(this IEnumerable<string> strings, string delimiter = " ")
    {
        return strings.Aggregate(string.Empty, (current, str) => current + str + delimiter);
    }

    public static TResult[] MergeArray<TSource, TResult>(this IEnumerable<TSource> array, Func<TSource, TResult> func)
    {
        return array.Select(elm => func(elm)).ToArray();
    }

    public static bool ContainsAny(this IEnumerable<object> objects, object[] objectsToEqual) => objects.Any(objectsToEqual.Contains);

    public static bool ContainsAnyToLower(this IEnumerable<string> strings, params string[] strs) =>
        strings.Select(x => x.ToLower()).ContainsAny(strs);
}