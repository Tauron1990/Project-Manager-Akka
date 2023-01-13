using Akka;
using Akka.Streams.Dsl;
using Tauron.Application.Akka.Redux.Configuration;

namespace Tauron.Application.Akka.Redux.Internal.Configuration;

public sealed class EffectFactory<TState> : IEffectFactory<TState>
    where TState : class, new()
{
    public IEffect<TState> CreateEffect(Func<Source<object, NotUsed>> run)
        => new SimpleEffect(run);

    public IEffect<TState> CreateEffect(Func<Source<TState, NotUsed>, Source<object, NotUsed>> run)
        => new SimpleStateEffect(run);

    public IEffect<TState> CreateEffect<TAction>(Func<Source<(TAction Action, TState State), NotUsed>, Source<object, NotUsed>> run) where TAction : class
        => new ActionStateEffect<TAction>(run);

    private sealed class SimpleEffect : IEffect<TState>
    {
        private readonly Func<Source<object, NotUsed>> _run;

        internal SimpleEffect(Func<Source<object, NotUsed>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(_run);
    }

    private sealed class SimpleStateEffect : IEffect<TState>
    {
        private readonly Func<Source<TState, NotUsed>, Source<object, NotUsed>> _run;

        internal SimpleStateEffect(Func<Source<TState, NotUsed>, Source<object, NotUsed>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(store => _run(store.Select()));
    }

    private sealed class ActionStateEffect<TAction> : IEffect<TState> where TAction : class
    {
        private readonly Func<Source<(TAction Action, TState State), NotUsed>, Source<object, NotUsed>> _run;

        internal ActionStateEffect(Func<Source<(TAction Action, TState State), NotUsed>, Source<object, NotUsed>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(
                s => _run(s.ObservActionState<TAction>().Select(a => (a.Action, a.State))));
    }
}