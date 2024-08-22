using Serilog;
using Serilog.Core;
using Serilog.Events;

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

    public static void Check(bool? boolean, string msg, Exception? exception = null)
    {
        if (boolean is not null && boolean.Value)
            return;

        Log.Error(msg);
        
        if (exception is not null)
            throw exception;
    }
}