using System;
using System.Reactive.PlatformServices;
using Microsoft.IO;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed class InMemoryRoot : DirectoryEntry
{
    public static readonly RecyclableMemoryStreamManager Manager = new();
    private readonly SimplePool<DirectoryEntry> _directoryPool = new(PoolConfig<DirectoryEntry>.Default);
    private readonly SimplePool<FileEntry> _filePool = new(PoolConfig<FileEntry>.Default);

    public FileEntry GetInitializedFile(string name, ISystemClock clock)
        => _filePool.Rent().Init(name, Manager.GetStream(), clock);

    public DirectoryEntry GetDirectoryEntry(string name, ISystemClock clock)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name Should not be null or Empty");

        DirectoryEntry dic = _directoryPool.Rent();
        dic.Init(name, clock);

        return dic;
    }

    public void ReturnFile(FileEntry entry)
        => _filePool.Return(entry);

    public void ReturnDirectory(DirectoryEntry entry)
    {
        if(entry.GetType() != typeof(DirectoryEntry))
            throw new InvalidOperationException("Invalid Directory Returned");

        foreach (IDataElement subEntry in entry.Elements)
            switch (subEntry)
            {
                case FileEntry ent:
                    ReturnFile(ent);

                    break;
                case DirectoryEntry dicEnt:
                    ReturnDirectory(dicEnt);

                    break;
            }

        _directoryPool.Return(entry);
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
        data.Init(data.ActualName, Manager.GetStream(), clock);
    }
}