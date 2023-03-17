using System;
using System.Reactive.PlatformServices;
using Microsoft.IO;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed class InMemoryRoot : DirectoryEntry
{
    private static readonly RecyclableMemoryStreamManager Manager = new();

    public FileEntry GetInitializedFile(string name, ISystemClock clock)
        => new(Manager.GetStream())
        {
            Name = name,
            ModifyDate = clock.UtcNow.ToLocalTime().DateTime,
        };

    public DirectoryEntry GetDirectoryEntry(string name, ISystemClock clock)
        => new()
        {
            Name = name,
            ModifyDate = clock.UtcNow.DateTime,
        };
}