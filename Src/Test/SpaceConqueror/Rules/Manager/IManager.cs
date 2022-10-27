using NRules.Fluent.Dsl;
using SpaceConqueror.States;

namespace SpaceConqueror.Rules.Manager;

public interface IManager<TManager, TState>
    where TManager : Rule, IManager<TManager, TState>
    where TState : IState
{
    
}