using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux.Internal;

internal class ExplicitStateLens<TState, TFeatureState> : BaseStateLens<TState, TFeatureState>
{
    private readonly Func<TState, TFeatureState> _featureSelector;

    private readonly Func<TState, TFeatureState, TState> _stateReducer;
    private readonly IMaterializer _materializer;

    public ExplicitStateLens(
        Func<TState, TFeatureState> featureSelector, 
        Func<TState, TFeatureState, TState> stateReducer,
        IMaterializer materializer)
    {
        _featureSelector = featureSelector;
        _stateReducer = stateReducer;
        _materializer = materializer;
    }

    protected override On<TState> CreateParentReducer(On<TFeatureState> on)
    {
        Source.sub
    }
}