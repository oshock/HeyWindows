namespace HeyWindows.Core.Utils;

public static class StringUtils
{
    public static string SubstringAfter(this string str, string delimiter)
    {
        var i = str.IndexOf(delimiter, StringComparison.Ordinal);
        return i > 0 ? str.Substring(i + delimiter.Length) : string.Empty;
    }

    public static string MakeFriendly(this string str) => str.Replace(" ", "_");
}