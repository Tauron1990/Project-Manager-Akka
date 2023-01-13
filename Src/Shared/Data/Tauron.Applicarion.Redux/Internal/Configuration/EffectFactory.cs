using Tauron.Applicarion.Redux.Configuration;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class EffectFactory<TState> : IEffectFactory<TState>
    where TState : class, new()
{
    public IEffect<TState> CreateEffect(Func<IObservable<object>> run)
        => new SimpleEffect(run);

    public IEffect<TState> CreateEffect(Func<IObservable<TState>, IObservable<object>> run)
        => new SimpleStateEffect(run);

    public IEffect<TState> CreateEffect<TAction>(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run) where TAction : class
        => new ActionStateEffect<TAction>(run);

    private sealed class SimpleEffect : IEffect<TState>
    {
        private readonly Func<IObservable<object>> _run;

        internal SimpleEffect(Func<IObservable<object>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(_run);
    }

    private sealed class SimpleStateEffect : IEffect<TState>
    {
        private readonly Func<IObservable<TState>, IObservable<object>> _run;

        internal SimpleStateEffect(Func<IObservable<TState>, IObservable<object>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(store => _run(store.Select()));
    }

    private sealed class ActionStateEffect<TAction> : IEffect<TState> where TAction : class
    {
        private readonly Func<IObservable<(TAction Action, TState State)>, IObservable<object>> _run;

        internal ActionStateEffect(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(
                s => _run(s.ObserveAction<TAction, (TAction, TState)>((action, state) => (action, state))));
    }
}