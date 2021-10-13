using System;
using System.Reactive.PlatformServices;
using MHLab.Pooling;
using Microsoft.IO;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed class InMemoryRoot : DirectoryEntry
{
    private readonly RecyclableMemoryStreamManager _manager = new();
    private readonly Pool<FileEntry> _filePool = new(0, () => new FileEntry());
    private readonly Pool<DirectoryEntry> _directoryPool = new(0, () => new DirectoryEntry());

    public FileEntry GetInitializedFile(string name, ISystemClock clock)
        => _filePool.Rent().Init(name, _manager.GetStream(), clock);

    public DirectoryEntry GetDirectoryEntry(string name, ISystemClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name Should not be null or Empty");
        var dic = _directoryPool.Rent();
        dic.Init(name, clock);

        return dic;
    }

    public void ReturnFile(FileEntry entry)
        => _filePool.Recycle(entry);

    public void ReturnDirectory(DirectoryEntry entry)
    {
        if (entry.GetType() != typeof(DirectoryEntry))
            throw new InvalidOperationException("Invalid Directory Returned");

        foreach (var subEntry in entry)
        {
            switch (subEntry)
            {
                case FileEntry ent:
                    ReturnFile(ent);
                    break;
                case DirectoryEntry dicEnt:
                    ReturnDirectory(dicEnt);
                    break;
            }
        }
        
        _directoryPool.Recycle(entry);
    }

    public override void Dispose()
    {
        base.Dispose();
        _filePool.Clear();
        _directoryPool.Clear();
    }

    public void ReInit(FileEntry data, ISystemClock clock)
    {
        data.ActualData.Dispose();
        data.Init(data.ActualName, _manager.GetStream(), clock);
    }
}