using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Configuration;

[PublicAPI]
public interface IEffectFactory<TState>
    where TState : class, new()
{
    IEffect<TState> CreateEffect(Func<IObservable<object>> run);

    IEffect<TState> CreateEffect(Func<IObservable<TState>, IObservable<object>> run);

    IEffect<TState> CreateEffect<TAction>(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run)
        where TAction : class;
}