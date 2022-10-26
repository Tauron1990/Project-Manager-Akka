using System.Collections.Immutable;
using Hyperion;
using NRules;

namespace SpaceConqueror.States;

public sealed class GlobalState
{
    private static readonly string SaveGamePath = Path.GetFullPath("saves");

    private readonly Serializer _serializer;
    private readonly ISessionFactory _sessionFactory;
    private readonly Func<IEnumerable<IState>> _newState;

    private List<IState> _gameState = new();
    private ISession _updater;
    
    public GlobalState(ISessionFactory sessionFactory, Func<IEnumerable<IState>> newState)
    {
        _sessionFactory = sessionFactory;
        _newState = newState;
        _updater = _sessionFactory.CreateSession();
    }

    private void Clear()
    {
        _gameState.Clear();
        _updater = _sessionFactory.CreateSession();
        
        GC.Collect();
    }
    
    public void NewGame()
    {
        Clear();
        
        _gameState.AddRange(_newState());
        _updater.InsertAll(_gameState);
    }

    public static IEnumerable<string> GetSaveGames()
        => Directory.Exists(SaveGamePath) 
            ? Directory.EnumerateFiles(SaveGamePath) 
            : Array.Empty<string>();

    public void SaveGame(string name)
    {
        if(!name.EndsWith(SaveGamePath))
            name = SaveGamePath + name;
        
        if(!name.EndsWith(".sav"))
            name += ".sav";

        using FileStream file = File.Open(name, FileMode.Create);
        _serializer.Serialize(
            new GameRecord(GameManager.GameVersion, _gameState.ToImmutableList()),
            file);
    }

    public void LoadGame(string name)
    {
        Clear();

        if(!File.Exists(name))
            throw new InvalidOperationException("Die Speicherdatei Existiert nicht");

        using FileStream file = File.Open(name, FileMode.Open);
        var record = _serializer.Deserialize<GameRecord>(file);

        _gameState.AddRange(record.GameState);
        
        if(GameManager.GameVersion != record.GameVersion)
        {
            SyncState();
        }
        
        _updater.InsertAll(_gameState);
    }

    private void SyncState()
    {
        foreach (IState state in _newState())
        {
            Type type = state.GetType();
            if(_gameState.Select(s => s.GetType()).Contains(type))
                continue;
            
            _gameState.Add(state);
            _updater.Insert(state);
        }
        
        GC.Collect();
    }
    
    private sealed record GameRecord(Version GameVersion, ImmutableList<IState> GameState);
}