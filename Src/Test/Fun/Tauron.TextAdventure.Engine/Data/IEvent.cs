namespace Tauron.TextAdventure.Engine.Data;

public interface IEvent : ISaveable
{
    long Sequence { get; }
}