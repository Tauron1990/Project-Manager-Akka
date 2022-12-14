using Tauron.TextAdventure.Engine.Core;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

public abstract class RoomBuilderBase
{
    protected internal abstract BaseRoom CreateRoom(AssetManager assetManager);
}