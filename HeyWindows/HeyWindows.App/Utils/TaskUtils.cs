namespace HeyWindows.App.Utils;

public static class TaskUtils
{
    public static void StartTaskWithDelay(Action action, int milliseconds)
    {
        new Task(() =>
        {
            Thread.Sleep(milliseconds);
            action();
        }).Start();
    }
}