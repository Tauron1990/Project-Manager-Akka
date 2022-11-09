namespace Tauron.Applicarion.Redux.Internal;

internal class ExplicitStateLens<TState, TFeatureState> : BaseStateLens<TState, TFeatureState>
{
    private readonly Func<TState, TFeatureState> _featureSelector;

    private readonly Func<TState, TFeatureState, TState> _stateReducer;

    public ExplicitStateLens(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer)
    {
        _featureSelector = featureSelector;
        _stateReducer = stateReducer;
    }

    protected override On<TState> CreateParentReducer(On<TFeatureState> on)
    {
        var on2 = on;

        return new On<TState>(
            (state, action) =>
            {
                TFeatureState val = _featureSelector(state);
                TFeatureState val2 = on2.Mutator(val, action);

                return val?.Equals(val2) == true ? state : _stateReducer(state, val2);
            },
            on2.ActionType
        );
    }
}