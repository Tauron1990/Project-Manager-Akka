using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using Tauron.TextAdventure.Engine.Systems.Actor;

namespace Tauron.TextAdventure.Engine.Data;

internal sealed class EventStore
{
    internal EventStore(string saveGameName)
    {
        GameState = new GameState();
        CurrentEvents = ImmutableSortedDictionary<int, IEvent>.Empty;
        SaveGameName = saveGameName;
    }

    private ImmutableSortedDictionary<int, IEvent> CurrentEvents { get; set; }

    internal GameState GameState { get; }

    private string SaveGameName { get; }
    
    private ZipArchive? GetSaveGameFile(bool read)
    {
        string dic = Path.GetFullPath(Path.Combine(GameHost.RootDic, "SaveGames"));
        if(!Directory.Exists(dic))
            Directory.CreateDirectory(dic);

        string gameFile = Path.Combine(dic, SaveGameName);


        if(read)
            return !File.Exists(gameFile)
                ? null
                : new ZipArchive(File.OpenRead(gameFile), ZipArchiveMode.Read);

        if(File.Exists(gameFile))
            File.Delete(gameFile);
            
        return new ZipArchive(File.Open(gameFile, FileMode.OpenOrCreate), ZipArchiveMode.Create);

    }

    internal Player LoadGame()
    {
        using ZipArchive? zip = GetSaveGameFile(read:true);

        ZipArchiveEntry? entry = zip?.Entries.FirstOrDefault(ent => string.Equals(ent.Name, "state", StringComparison.Ordinal));
        if(entry is null) return GameState.Get<Player>();

        using Stream entryStream = entry.Open();
        ((ISaveable)GameState).Read(new BinaryReader(entryStream));

        return GameState.Get<Player>();
    }

    internal void SaveGame()
    {
        using ZipArchive? zip = GetSaveGameFile(read:false);

        if(zip is null)
            #pragma warning disable EX002
            throw new UnreachableException();
        #pragma warning restore EX002

        ZipArchiveEntry state = zip.CreateEntry("state", CompressionLevel.Optimal);

        using (Stream entryStream = state.Open())
            ((ISaveable)GameState).Write(new BinaryWriter(entryStream));

        long eventsName = DateTime.UtcNow.Ticks;

        ZipArchiveEntry eventsEntry = zip.CreateEntry(eventsName.ToString(CultureInfo.InvariantCulture), CompressionLevel.Optimal);
        using Stream stream = eventsEntry.Open();
        using var writer = new BinaryWriter(stream);

        writer.Write(CurrentEvents.Count);

        foreach (var currentEvent in CurrentEvents)
        {
            writer.Write(currentEvent.Key);
            #pragma warning disable EX002
            writer.Write(currentEvent.Value.GetType().AssemblyQualifiedName ?? throw new UnreachableException());
            #pragma warning restore EX002
            currentEvent.Value.Write(writer);
        }

        CurrentEvents = ImmutableSortedDictionary<int, IEvent>.Empty;
    }

    internal void ApplyEvent<TEvent>(TEvent evt, Action<GameState, TEvent>? applyState)
        where TEvent : IEvent
    {
        GameState.Sequence++;
        IEvent newEvent = evt.WithSequence(GameState.Sequence);

        CurrentEvents = CurrentEvents.Add(newEvent.Sequence, newEvent);

        applyState?.Invoke(GameState, (TEvent)newEvent);
    }
}