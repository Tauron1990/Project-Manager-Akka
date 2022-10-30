namespace SpaceConqueror.States.Rooms;

public class RoomState
{
    public const string FailRoom = "InternalFailRoom";
    
    public string Id { get; }

    public bool IsPlayerInRoom { get; set; }
    
    public bool ExcludeHistory { get; set; }
    
    public RoomState(string id)
        => Id = id;
}