using ReduxSimple;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class ConfiguratedState : IConfiguredState
{
    private readonly IEnumerable<On<ApplicationState>> _reducers;
    private readonly IEnumerable<IEffect> _effects;
    private readonly Type _stateType;
    private readonly Guid _id;

    public ConfiguratedState(IEnumerable<On<ApplicationState>> reducers, IEnumerable<IEffect> effects, Type stateType, Guid id)
    {
        _reducers = reducers;
        _effects = effects;
        _stateType = stateType;
        _id = id;
    }
    
    public void RunConfig(ReduxStore<ApplicationState> store, Action<Type, Guid> registerState)
    {
        registerState(_stateType, _id);
        store.AddReducers(_reducers.ToArray());
        store.RegisterEffects(_effects.Select(e => e.Build()).ToArray());
    }
}