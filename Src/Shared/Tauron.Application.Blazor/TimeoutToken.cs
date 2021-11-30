using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.Blazor;

[PublicAPI]
public sealed class TimeoutToken
{
    #pragma warning disable CA1068
    public static ValueTask<TResult> WithDefault<TResult>(CancellationToken token, Func<CancellationToken, Task<TResult>> runner)
        => With(TimeSpan.FromSeconds(20), token, runner);
    
    public static async ValueTask<TResult> With<TResult>(TimeSpan timeSpan, CancellationToken token, Func<CancellationToken, Task<TResult>> runner)
        #pragma warning restore CA1068
    {
        using var cts = CreateSource();
        
        return await runner(cts.Token);

        CancellationTokenSource CreateSource()
        {
            var timeout =
                    #if DEBUG
                    TimeSpan.FromSeconds(300)
                    #else
                    timeSpan
                    #endif
                ;

            if (token.CanBeCanceled)
            {
                var source = CancellationTokenSource.CreateLinkedTokenSource(token);
                source.CancelAfter(timeout);

                return source;
            }
            else
                return new CancellationTokenSource(timeout);
        }
    }
}