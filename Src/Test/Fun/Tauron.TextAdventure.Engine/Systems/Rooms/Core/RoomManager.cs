using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Tauron.TextAdventure.Engine.Systems.Actor;
using Tauron.TextAdventure.Engine.UI;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Core;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class RoomManager : ISystem
{
    private readonly RoomMap _map;

    [SuppressMessage("Gu.Analyzers.Correctness", "GU0073:Member of non-public type should be internal")]
    public RoomManager(RoomMap map)
        => _map = map;

    public IEnumerable<IDisposable> Initialize(EventManager eventManager)
    {
        var renderState = eventManager.GameState.Get<RenderState>();

        yield return eventManager.GameState.Get<Player>()
           .Location
           .Where(s => !string.IsNullOrWhiteSpace(s))
           .Select(_map.Get)
           .Subscribe(
                r =>
                {
                    renderState.ToRender.Set(nameof(RoomManager), r.CreateRender());
                    renderState.Commands.Set(nameof(RoomManager), r.CreateCommands());
                });

        yield return eventManager
            .OnCommand<MoveToRoomCommand>()
            .Subscribe(evt => eventManager.StoreEvent(new MoveToRoomEvent(evt.RoomName)));

        yield return (
            from evt in eventManager.OnEvent<MoveToRoomEvent>()
            let player = eventManager.GameState.Get<Player>()
            select (player.Location, evt.TargetRoom)
        ).Subscribe(d => d.Location.Value = d.TargetRoom);
    }
}