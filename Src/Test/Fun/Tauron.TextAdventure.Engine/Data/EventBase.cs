namespace Tauron.TextAdventure.Engine.Data;

public abstract class EventBase : IEvent
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(Sequence);
        WriteInternal(writer);
    }

    public void Read(BinaryReader reader)
    {
        Sequence = reader.ReadInt32();
        ReadInternal(reader);
    }

    protected abstract void WriteInternal(BinaryWriter writer);

    protected abstract void ReadInternal(BinaryReader reader);
    
    public int Sequence { get; private set; }

    public IEvent WithSequence(int value)
    {
        Sequence = value;

        return this;
    }
}