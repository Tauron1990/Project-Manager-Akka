namespace Tauron.Application.Akka.Redux.Internal;

public interface IInternalRootStoreState<TState>
{
    IReduxStore<TState> Store { get; }
}