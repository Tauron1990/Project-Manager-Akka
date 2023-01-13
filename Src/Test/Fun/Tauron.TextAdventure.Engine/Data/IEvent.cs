namespace Tauron.TextAdventure.Engine.Data;

public interface IEvent : ISaveable
{
    int Sequence { get; }

    IEvent WithSequence(int value);
}