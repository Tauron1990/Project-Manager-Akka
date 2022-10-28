using System.Collections.Concurrent;
using NRules.Fluent.Dsl;
using SpaceConqueror.Rules.Rooms;
using SpaceConqueror.States;

namespace SpaceConqueror.Modules;

public sealed class RoomRegistrar
{
    private readonly ConcurrentDictionary<string, RoomBuilder> _rules = new();

    internal RoomRegistrar(){}

    public RoomBuilder Register<TState, TManager>(string name)
        where TState : IState, new()
        where TManager : Rule, IRoomManager<TManager>
    {
        var builder = new RoomBuilder(name);

        if(!_rules.TryAdd(name, builder))
            throw new InvalidOperationException("The Room is already Registrated");

        builder.States.Add<TState>();
        builder.Rules.Add(typeof(TManager));
        return builder;

    }

    public RoomBuilder GetRoom(string name) => _rules[name];
    
    public IEnumerable<(IEnumerable<Type> Rules, Func<IEnumerable<IState>> States)> GetRules() 
        => _rules.Values.Select(rb => rb.Init());
}