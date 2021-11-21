using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.Blazor;

[PublicAPI]
public sealed class TimeoutToken
{
    public static ValueTask<TResult> WithDefault<TResult>(Func<CancellationToken, ValueTask<TResult>> runner)
        => With(TimeSpan.FromSeconds(20), runner);

    public static async ValueTask<TResult> With<TResult>(TimeSpan timeSpan, Func<CancellationToken, ValueTask<TResult>> runner)
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