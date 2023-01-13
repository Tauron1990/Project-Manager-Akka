using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.Systems.Rooms;

public sealed class MoveToRoomEvent : EventBase
{
    public MoveToRoomEvent() { }

    public MoveToRoomEvent(string targetRoom)
        => TargetRoom = targetRoom;

    public string TargetRoom { get; set; } = string.Empty;

    protected override void WriteInternal(BinaryWriter writer)
        => writer.Write(TargetRoom);

    protected override void ReadInternal(BinaryReader reader)
        => TargetRoom = reader.ReadString();
}