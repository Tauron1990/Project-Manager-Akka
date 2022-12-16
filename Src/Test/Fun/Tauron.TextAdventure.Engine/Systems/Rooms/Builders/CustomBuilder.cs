using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Core;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

[PublicAPI]
public sealed class CustomBuilder : RoomBuilderBase
{
    private Func<AssetManager, BaseRoom>? _roomBuilder;
    private Action<AssetManager, BaseRoom>? _modify;

    public CustomBuilder WithFactory(Func<AssetManager,BaseRoom> factory)
    {
        _roomBuilder = factory;
        return this;
    }

    public CustomBuilder WithModify(Action<AssetManager, BaseRoom> mod)
    {
        
        if(_modify is null)
            _modify = mod;
        else
            _modify += mod;

        return this;
    }
    
    protected internal override BaseRoom CreateRoom(AssetManager assetManager)
    {
        if(_roomBuilder is null)
            throw new InvalidOperationException("No Factory for Custom Room Provided");

        BaseRoom room = _roomBuilder(assetManager);
        if(_modify is not null)
            _modify(assetManager, room);

        return room;
    }
}