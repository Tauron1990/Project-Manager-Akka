using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.Systems.Rooms;

public sealed class MoveToRoomEvent : EventBase
{
    public string TargetRoom { get; set; } = string.Empty;

    public MoveToRoomEvent()
    { }

    public MoveToRoomEvent(string targetRoom)
        => TargetRoom = targetRoom;

    protected override void WriteInternal(BinaryWriter writer)
        => writer.Write(TargetRoom);

    protected override void ReadInternal(BinaryReader reader)
        => TargetRoom = reader.ReadString();
}