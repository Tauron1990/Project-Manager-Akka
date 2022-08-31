namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public static class To
{
    public static ValueTask<T> Task<T>(Func<T> runner)
    {
        try
        {
            return ValueTask.FromResult<T>(runner());
        }
        catch (Exception e)
        {
            return ValueTask.FromException<T>(e);
        }
    }
    
    public static ValueTask TaskV(Action runner)
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
}