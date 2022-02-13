﻿using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public sealed class TimeoutToken
{
    #pragma warning disable CA1068
    public static ValueTask WithDefault(CancellationToken token, Func<CancellationToken, Task> runner)
        => With(TimeSpan.FromSeconds(20), token, runner);

    public static async ValueTask With(TimeSpan timeSpan, CancellationToken token, Func<CancellationToken, Task> runner)
        => await With(timeSpan, token,
            async timeoutToken =>
            {
                await runner(timeoutToken);
                return Unit.Default;
            });
    
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
                    TimeSpan.FromMinutes(300)
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