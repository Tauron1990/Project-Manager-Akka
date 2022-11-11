using System.Diagnostics;

namespace Tauron.Features;

[DebuggerStepThrough]
internal sealed class FeatureImpl<TState> : ActorBuilder<TState>.ConvertingFeature<TState, GenericState>
    where TState : notnull
{
    internal FeatureImpl(IFeature<TState> target)
        : base(
            target,
            state => (TState)state.States[typeof(TState)],
            (original, state) => original with { States = original.States.SetItem(typeof(TState), state) }) { }
}