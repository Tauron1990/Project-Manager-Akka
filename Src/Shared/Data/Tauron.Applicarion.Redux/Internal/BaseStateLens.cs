namespace Tauron.Applicarion.Redux.Internal;

internal abstract class BaseStateLens<TState, TFeatureState> : IStateLens<TState, TFeatureState>
{
    private readonly List<On<TState>> _ons = new();

    public IStateLens<TState, TFeatureState> On<TAction>(Func<TFeatureState, TAction, TFeatureState> featureReducer) where TAction : class
    {
        _ons.Add(CreateParentReducer(Create.On(featureReducer)));

        return this;
    }

    public IStateLens<TState, TFeatureState> On<TAction>(Func<TFeatureState, TFeatureState> featureReducer) where TAction : class
    {
        _ons.Add(CreateParentReducer(Create.On<TAction, TFeatureState>(featureReducer)));

        return this;
    }

    public IEnumerable<On<TState>> ToList()
        => _ons.AsEnumerable();

    protected abstract On<TState> CreateParentReducer(On<TFeatureState> on);
}