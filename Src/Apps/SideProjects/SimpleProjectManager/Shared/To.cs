namespace SimpleProjectManager.Shared;

public static class To
{
    public static ValueTask<T> VTask<T>(Func<T> runner)
    {
        try
        {
            return ValueTask.FromResult(runner());
        }
        catch (Exception e)
        {
            return ValueTask.FromException<T>(e);
        }
    }

    public static ValueTask VTaskV(Action runner)
    {
        try
        {
            runner();

            return ValueTask.CompletedTask;
        }
        catch (Exception e)
        {
            return ValueTask.FromException(e);
        }
    }

    public static Task<T> Task<T>(Func<T> runner)
    {
        try
        {
            return System.Threading.Tasks.Task.FromResult(runner());
        }
        catch (Exception e)
        {
            return System.Threading.Tasks.Task.FromException<T>(e);
        }
    }

    public static Task TaskV(Action runner)
    {
        try
        {
            runner();

            return System.Threading.Tasks.Task.CompletedTask;
        }
        catch (Exception e)
        {
            return System.Threading.Tasks.Task.FromException(e);
        }
    }
}