using System.Collections.Concurrent;
using NRules.Fluent.Dsl;
using SpaceConqueror.Rules.Manager;
using SpaceConqueror.States;

namespace SpaceConqueror.Modules;

public sealed class ManagerRegistrar
{
    private readonly ConcurrentDictionary<Type, Type[]> _rules = new();
    private readonly StateRegistrar _stateRegistrar;

    public ManagerRegistrar(StateRegistrar stateRegistrar)
        => _stateRegistrar = stateRegistrar;

    public void RegisterManager<TState, TManager>()
        where TState : IState, new()
        where TManager : Rule, IManager<TManager, TState>
    {
        _rules[typeof(TState)] = new []{ typeof(TManager) };
        _stateRegistrar.Add<TState>();
    }
    
    public void RegisterManager<TState, TManager, TManager2>()
        where TState : IState, new()
        where TManager : Rule, IManager<TManager, TState>
        where TManager2 : Rule, IManager<TManager2, TState>
    {
        _rules[typeof(TState)] = new []{ typeof(TManager), typeof(TManager2) };
        _stateRegistrar.Add<TState>();
    }
    
    public void RegisterManager<TState, TManager>(Func<TState> stateFactory)
        where TState : IState
        where TManager : Rule, IManager<TManager, TState>
    {
        _rules[typeof(TState)] = new []{ typeof(TManager) };
        _stateRegistrar.Add(stateFactory);
    }

    internal IEnumerable<Type> GetRules()
        => _rules.Values.SelectMany(l => l);
}