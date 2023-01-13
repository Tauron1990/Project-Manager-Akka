namespace Tauron.Applicarion.Redux.Internal;

public interface IInternalRootStoreState<TState>
{
    IReduxStore<TState> Store { get; }
}