﻿using ReduxSimple;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class ConfiguratedState : IConfiguredState
{
    private readonly IEnumerable<On<ApplicationState>> _reducers;
    private readonly IEnumerable<IEffect> _effects;
    private readonly Type _stateType;
    private readonly Guid _id;
    private readonly Action<IRootStore>? _onCreate;

    public ConfiguratedState(IEnumerable<On<ApplicationState>> reducers, IEnumerable<IEffect> effects, Type stateType, Guid id, Action<IRootStore>? onCreate)
    {
        _reducers = reducers;
        _effects = effects;
        _stateType = stateType;
        _id = id;
        _onCreate = onCreate;
    }
    
    public void RunConfig(ReduxStore<ApplicationState> store, Action<Type, Guid> registerState)
    {
        registerState(_stateType, _id);
        store.AddReducers(_reducers.ToArray());
        store.RegisterEffects(_effects.Select(e => e.Build()).ToArray());
    }

    public void PostBuild(IRootStore store)
        => _onCreate?.Invoke(store);
}