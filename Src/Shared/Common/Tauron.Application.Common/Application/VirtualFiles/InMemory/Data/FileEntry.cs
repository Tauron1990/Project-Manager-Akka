using System;
using System.IO;
using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed class FileEntry : DataElementBase
{
    public MemoryStream? Data { get; private set; }

    public string ActualName
    {
        get
        {
            if(string.IsNullOrWhiteSpace(Name))
                ThrowNotInitException();

            return Name;
        }
    }

    public MemoryStream ActualData
    {
        get
        {
            if(Data is null)
                ThrowNotInitException();

            return Data!;
        }
    }

    public FileEntry Init(string name, MemoryStream stream, ISystemClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name dhould not be null", nameof(name));
            
        Name = name;
        Data = stream;
        CreationDate = clock.UtcNow.LocalDateTime;
        ModifyDate = clock.UtcNow.LocalDateTime; 
            
        return this;
    }
        
    public override void Dispose()
    {
        base.Dispose();
            
        Data?.Dispose();
            
        Data = null;
        Name = string.Empty;
    }

    private static void ThrowNotInitException()
        => throw new InvalidOperationException("Entry not initialized");
}