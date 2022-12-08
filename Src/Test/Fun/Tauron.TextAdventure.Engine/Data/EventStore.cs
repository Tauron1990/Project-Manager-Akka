using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;

namespace Tauron.TextAdventure.Engine.Data;

internal sealed class EventStore
{
    private ImmutableDictionary<int, IEvent> CurrentEvents { get; set; }

    internal GameState GameState { get; }

    private string SaveGameName { get; }

    internal EventStore(string saveGameName)
    {
        GameState = new GameState();
        CurrentEvents = ImmutableDictionary<int, IEvent>.Empty;
        SaveGameName = saveGameName;
    }

    private ZipArchive? GetSaveGameFile(ZipArchiveMode mode)
    {
        string dic = Path.GetFullPath("SaveGames");
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

        ZipArchiveEntry? entry = zip.Entries.FirstOrDefault(ent => string.Equals(ent.Name, "state", StringComparison.Ordinal));
        if(entry is null) return;

        using Stream entryStream = entry.Open();
        ((ISaveable)GameState).Read(new BinaryReader(entryStream));
    }

    internal void SaveGame()
    {
        using ZipArchive? zip = GetSaveGameFile(ZipArchiveMode.Update);

        if(zip is null)
            throw new UnreachableException();

        ZipArchiveEntry? state = zip.Entries.FirstOrDefault(ent => string.Equals(ent.Name, "state", StringComparison.Ordinal));
    }
}