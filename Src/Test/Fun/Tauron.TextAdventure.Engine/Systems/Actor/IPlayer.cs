using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.Systems.Actor;

public sealed class Player : IActor
{
    public string Id { get; } = "Player";

    public string Location { get; set; } = string.Empty;
    
    public string PlayerName { get; set; } = string.Empty;

    void ISaveable.Write(BinaryWriter writer)
    {
        writer.Write(Location);
        writer.Write(PlayerName);
    }

    void ISaveable.Read(BinaryReader reader)
    {
        Location = reader.ReadString();
        PlayerName = reader.ReadString();
    }
}