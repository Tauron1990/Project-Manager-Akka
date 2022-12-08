namespace Tauron.TextAdventure.Engine.Data;

public sealed class GameState : ISaveable
{
    private readonly Dictionary<Type, IState> _states = new();

    public TState Get<TState>()
        where TState : IState, new()
    {
        if(_states.TryGetValue(typeof(TState), out var state))
            return (TState)state;

        var newState = new TState();
        _states[typeof(TState)] = newState;

        return newState;
    }

    void ISaveable.Write(BinaryWriter writer)
    {
        writer.Write(_states.Count);
        foreach (var state in _states)
        {
            writer.Write(state.Key.AssemblyQualifiedName);
            state.Value.Write(writer);
        }
    }

    void ISaveable.Read(BinaryReader reader)
    {
        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            string typeString = reader.ReadString();
            var type = Type.GetType(typeString, true);
            
            var state = (IState)Activator.CreateInstance(type!)!;
            
            state.Read(reader);
            _states[type] = state;
        }
    }
}