namespace Tauron.TextAdventure.Engine.Data;

public interface ISaveable
{
    void Write(BinaryWriter writer);

    void Read(BinaryReader reader);
}