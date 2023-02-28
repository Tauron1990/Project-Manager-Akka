using System.Diagnostics;

namespace Tauron.Features;

[DebuggerStepThrough]
internal sealed class FeatureImpl<TState> : ActorBuilder<TState>.ConvertingFeature<TState, GenericState>
    where TState : notnull
{
    internal FeatureImpl(IFeature<TState> target)
        : base(target, Convert, ConvertBack) { }

    private static TState Convert(GenericState state) => (TState)state.States[typeof(TState)];

    private static GenericState ConvertBack(GenericState original, TState state)
    {
        original.States[typeof(TState)] = state;
        
        return original;
    }
}