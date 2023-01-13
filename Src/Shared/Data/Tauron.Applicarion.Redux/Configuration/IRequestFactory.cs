using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Applicarion.Redux.Configuration;

[PublicAPI]
public interface IRequestFactory<TState>
{
    IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess)
        where TAction : class;

    IRequestFactory<TState> AddRequest<TAction>(
        Func<TAction, CancellationToken, ValueTask<SimpleResult>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class;

    IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess)
        where TAction : class;

    IRequestFactory<TState> AddRequest<TAction>(
        Func<TAction, CancellationToken, Task<SimpleResult>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class;

    IRequestFactory<TState> OnTheFlyUpdate<TSource, TData>(
        Func<TState, TSource> sourceSelector,
        Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher,
        Func<TState, TData, TState> patcher);

    IRequestFactory<TState> OnTheFlyUpdate<TData>(
        Func<CancellationToken, Task<TData>> fetcher,
        Func<TState, TData, TState> patcher);
}