namespace SimpleProjectManager.Client;

public sealed class TimeoutToken
{
    public static async ValueTask<TResult> With<TResult>(TimeSpan timeSpan, Func<CancellationToken, Task<TResult>> runner)
    {
        using var cts = new CancellationTokenSource(timeSpan);
        return await runner(cts.Token);
    }
}