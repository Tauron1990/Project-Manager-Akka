using System.Collections.Concurrent;
using JetBrains.Annotations;
using NRules.Fluent.Dsl;
using SpaceConqueror.Core;
using SpaceConqueror.Rules.Rooms;
using SpaceConqueror.States;

namespace SpaceConqueror.Modules;

[PublicAPI]
public sealed class RoomRegistrar
{
    private readonly AssetManager _assetManager;
    private readonly ConcurrentDictionary<string, RoomBuilder> _rules = new();

    internal RoomRegistrar(AssetManager assetManager)
        => _assetManager = assetManager;

    public RoomBuilder Register<TState, TManager>(string name)
        where TState : IState, new()
        where TManager : Rule, IRoomManager<TManager>
    {
        var builder = new RoomBuilder(name, _assetManager);

        if(!_rules.TryAdd(name, builder))
            throw new InvalidOperationException("The Room is already Registrated");

        builder.States.Add<TState>();
        builder.Rules.Add(typeof(TManager));

        return builder;

    }

    public RoomBuilder Register<TState>(string name)
        where TState : IState, new()
    {
        var builder = new RoomBuilder(name, _assetManager);

        if(!_rules.TryAdd(name, builder))
            throw new InvalidOperationException("The Room is already Registrated");

        builder.States.Add<TState>();

        return builder;
    }

    public RoomBuilder Register(string name)
    {
        var builder = new RoomBuilder(name, _assetManager);

        if(!_rules.TryAdd(name, builder))
            throw new InvalidOperationException("The Room is already Registrated");

        return builder;
    }

    public RoomBuilder GetRoom(string name) => _rules[name];

    public IEnumerable<(IEnumerable<Type> Rules, Func<IEnumerable<IState>> States)> GetRules()
        => _rules.Values.Select(rb => rb.Init());
}