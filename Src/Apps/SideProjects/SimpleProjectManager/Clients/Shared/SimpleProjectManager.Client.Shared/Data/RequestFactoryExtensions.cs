using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Shared.Services;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data;

public static class RequestFactoryExtensions
{
    public static IRequestFactory<TState> AddRequest<TState, TAction>(
        this IRequestFactory<TState> factory,
        Func<TAction, CancellationToken, ValueTask<SimpleResultContainer>> runRequest,
        Func<TState, TAction, TState> onScess)
        where TAction : class
    {
        async ValueTask<SimpleResult> RunRequest(TAction a, CancellationToken c) =>
            (await runRequest(a, c).ConfigureAwait(false)).SimpleResult;
        
        return factory.AddRequest(RunRequest, onScess);
    }

    public static IRequestFactory<TState> AddRequest<TState, TAction>(
        this IRequestFactory<TState> factory,
        Func<TAction, CancellationToken, ValueTask<SimpleResultContainer>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        async ValueTask<SimpleResult> RunRequest(TAction a, CancellationToken c) =>
            (await runRequest(a, c).ConfigureAwait(false)).SimpleResult;

        return factory.AddRequest(RunRequest, onScess, onFail);
    }

    public static IRequestFactory<TState> AddRequest<TState, TAction>(
        this IRequestFactory<TState> factory,
        Func<TAction, CancellationToken, Task<SimpleResultContainer>> runRequest,
        Func<TState, TAction, TState> onScess)
        where TAction : class
    {
        async ValueTask<SimpleResult> RunRequest(TAction a, CancellationToken c) =>
            (await runRequest(a, c).ConfigureAwait(false)).SimpleResult;

        return factory.AddRequest(RunRequest, onScess);
    }

    public static IRequestFactory<TState> AddRequest<TState, TAction>(
        this IRequestFactory<TState> factory,
        Func<TAction, CancellationToken, Task<SimpleResultContainer>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        async ValueTask<SimpleResult> RunRequest(TAction a, CancellationToken c) =>
            (await runRequest(a, c).ConfigureAwait(false)).SimpleResult;

        return factory.AddRequest(RunRequest, onScess, onFail);
    }
    
}