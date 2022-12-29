using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.Systems.Actor;

[PublicAPI]
public sealed class Player : IActor
{
    public ReactiveProperty<string> PlayerName { get; set; } = new(string.Empty);
    public string Id => "Player";

    public ReactiveProperty<string> Location { get; set; } = new(string.Empty);

    void ISaveable.Write(BinaryWriter writer)
    {
        writer.Write(Location.Value);
        writer.Write(PlayerName.Value);
    }

    void ISaveable.Read(BinaryReader reader)
    {
        Location.Value = reader.ReadString();
        PlayerName.Value = reader.ReadString();
    }
}