using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;

namespace Tauron.TextAdventure.Engine.Data;

internal sealed class EventStore
{
    private ImmutableSortedDictionary<int, IEvent> CurrentEvents { get; set; }

    internal GameState GameState { get; }

    private string SaveGameName { get; }

    internal EventStore(string saveGameName)
    {
        GameState = new GameState();
        CurrentEvents = ImmutableSortedDictionary<int, IEvent>.Empty;
        SaveGameName = saveGameName;
    }

    private ZipArchive? GetSaveGameFile(ZipArchiveMode mode)
    {
        string dic = Path.GetFullPath(Path.Combine(GameHost.RootDic, "SaveGames"));
        if(!Directory.Exists(dic))
            Directory.CreateDirectory(dic);

        string gameFile = Path.Combine(dic, SaveGameName);


        if(mode != ZipArchiveMode.Read)
            return File.Exists(gameFile) 
                ? new ZipArchive(File.Open(gameFile, FileMode.OpenOrCreate), mode) 
                : new ZipArchive(File.Open(gameFile, FileMode.OpenOrCreate), ZipArchiveMode.Create);
        
        return !File.Exists(gameFile) 
            ? null 
            : new ZipArchive(File.OpenRead(gameFile), mode);

    }
    
    internal void LoadGame()
    {
        using ZipArchive? zip = GetSaveGameFile(ZipArchiveMode.Read);

        ZipArchiveEntry? entry = zip?.Entries.FirstOrDefault(ent => string.Equals(ent.Name, "state", StringComparison.Ordinal));
        if(entry is null) return;

        using Stream entryStream = entry.Open();
        ((ISaveable)GameState).Read(new BinaryReader(entryStream));
    }

    internal void SaveGame()
    {
        using ZipArchive? zip = GetSaveGameFile(ZipArchiveMode.Update);

        if(zip is null)
            throw new UnreachableException();

        ZipArchiveEntry state = zip.Entries.FirstOrDefault(ent => string.Equals(ent.Name, "state", StringComparison.Ordinal)) 
                             ?? zip.CreateEntry("state", CompressionLevel.Optimal);

        using Stream entryStream = state.Open();
        ((ISaveable)GameState).Write(new BinaryWriter(entryStream));

        long eventsName = DateTime.UtcNow.Ticks;

        ZipArchiveEntry eventsEntry = zip.CreateEntry(eventsName.ToString(CultureInfo.InvariantCulture), CompressionLevel.Optimal);
        using Stream stream = eventsEntry.Open();
        using BinaryWriter writer = new BinaryWriter(stream);
        
        writer.Write(CurrentEvents.Count);

        foreach (var currentEvent in CurrentEvents)
        {
            writer.Write(currentEvent.Key);
            writer.Write(currentEvent.Value.GetType().AssemblyQualifiedName ?? throw new UnreachableException());
            currentEvent.Value.Write(writer);
        }
        
        CurrentEvents = ImmutableSortedDictionary<int, IEvent>.Empty;
    }

    public void ApplyEvent<TEvent>(TEvent evt, Action<GameState, TEvent>? applyState)
    where TEvent : IEvent
    {
        GameState.Sequence++;
        IEvent newEvent = evt.WithSequence(GameState.Sequence);

        CurrentEvents = CurrentEvents.Add(newEvent.Sequence, newEvent);

        applyState?.Invoke(GameState, (TEvent)newEvent);
    }
}