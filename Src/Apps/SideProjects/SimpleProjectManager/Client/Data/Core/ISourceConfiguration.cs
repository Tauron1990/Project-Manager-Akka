using JetBrains.Annotations;

namespace SimpleProjectManager.Client.Data.Core;

[PublicAPI]
public interface ISourceConfiguration<TActualState> where TActualState : class, new()
{
    IStateConfiguration<TActualState> FromInitial(TActualState? initial = default);

    IStateConfiguration<TActualState> FromServer(Func<CancellationToken, Task<TActualState>> fetcher);
    
    IStateConfiguration<TActualState> FromServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TActualState, TToPatch, TActualState> patcher);

    IStateConfiguration<TActualState> FromCacheAndServer(Func<CancellationToken, Task<TActualState>> fetcher);
    
    IStateConfiguration<TActualState> FromCacheAndServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TActualState, TToPatch, TActualState> patcher);
}