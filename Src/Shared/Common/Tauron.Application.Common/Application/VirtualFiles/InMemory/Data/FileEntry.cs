using System;
using System.IO;
using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed class FileEntry : DataElementBase
{
    public MemoryStream Data { get; }

    public string ActualName => Name;

    public MemoryStream ActualData => Data!;

    public FileEntry(MemoryStream data)
    {
        Data = data;
    }

    public override void Dispose() => Data.Dispose();
}