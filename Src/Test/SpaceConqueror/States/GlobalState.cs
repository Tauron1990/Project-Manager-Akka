using System.Collections.Immutable;
using Hyperion;
using NRules;

namespace SpaceConqueror.States;

public sealed class GlobalState
{
    private static readonly string SaveGamePath = Path.GetFullPath("saves");
    private readonly List<IState> _gameState = new();
    private readonly Func<IEnumerable<IState>> _newState;

    private readonly Serializer _serializer = new();
    private readonly ISessionFactory _sessionFactory;

    public GlobalState(ISessionFactory sessionFactory, Func<IEnumerable<IState>> newState)
    {
        _sessionFactory = sessionFactory;
        _newState = newState;
        Updater = _sessionFactory.CreateSession();
    }

    public ISession Updater { get; private set; }

    private void Clear()
    {
        _gameState.Clear();
        Updater = _sessionFactory.CreateSession();

        GC.Collect();
    }

    public void NewGame()
    {
        Clear();

        Updater.InsertAll(_newState());
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

        _gameState.Clear();
        _gameState.AddRange(Updater.Query<IState>());

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

        if(GameManager.GameVersion != record.GameVersion)
        {
            _gameState.AddRange(record.GameState);
            SyncState();
        }

        Updater.InsertAll(_gameState);
    }

    private void SyncState()
    {
        foreach (IState state in _newState())
        {
            Type type = state.GetType();

            if(_gameState.Select(s => s.GetType()).Contains(type))
                continue;

            _gameState.Add(state);
            Updater.Insert(state);
        }

        GC.Collect();
    }

    private sealed record GameRecord(Version GameVersion, ImmutableList<IState> GameState);
}