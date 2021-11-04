namespace SimpleProjectManager.Client;

public sealed class TimeoutToken
{
    public static ValueTask<TResult> WithDefault<TResult>(Func<CancellationToken, Task<TResult>> runner)
        => With(TimeSpan.FromSeconds(20), runner);

    public static async ValueTask<TResult> With<TResult>(TimeSpan timeSpan, Func<CancellationToken, Task<TResult>> runner)
    {
        using var cts = new CancellationTokenSource
            (
            #if DEBUG
            TimeSpan.FromSeconds(300)
            #else
            timeSpan
            #endif
            );
        return await runner(cts.Token);
    }
}