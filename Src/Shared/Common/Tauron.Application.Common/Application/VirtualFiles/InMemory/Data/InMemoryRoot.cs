using System.Reactive.PlatformServices;
using Microsoft.IO;
using Tauron.Errors;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed class InMemoryRoot : DirectoryEntry
{
    public static readonly RecyclableMemoryStreamManager Manager = new();
    private readonly SimplePool<DirectoryEntry> _directoryPool = new(PoolConfig<DirectoryEntry>.Default);
    private readonly SimplePool<FileEntry> _filePool = new(PoolConfig<FileEntry>.Default);

    public Result<FileEntry> GetInitializedFile(string name, ISystemClock clock)
        => _filePool.Rent().Init(name, Manager.GetStream(), clock);

    public Result<DirectoryEntry> GetDirectoryEntry(string name, ISystemClock clock)
    {
        if(string.IsNullOrWhiteSpace(name))
            return new NullOrEmpty(nameof(name));

        return Result.Try(() => _directoryPool.Rent())
            .Bind(dic => dic.Init(name, clock));
    }

    public void ReturnFile(FileEntry entry)
        => _filePool.Return(entry);

    public void ReturnDirectory(DirectoryEntry entry)
    {
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

    public Result<FileEntry> ReInit(FileEntry data, ISystemClock clock)
    {
        data.Data?.Dispose();
        return data.ActualName.Bind(name =>  data.Init(name, Manager.GetStream(), clock));
    }
}