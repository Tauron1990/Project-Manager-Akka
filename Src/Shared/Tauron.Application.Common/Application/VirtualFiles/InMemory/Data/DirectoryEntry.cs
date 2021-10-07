using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public class DirectoryEntry : DataElementBase
{
    private readonly ConcurrentDictionary<string, IDataElement> _elements = new();

    public IEnumerable<FileEntry> Files => _elements.Values.OfType<FileEntry>();

    public IEnumerable<DirectoryEntry> Directorys => _elements.Values.OfType<DirectoryEntry>();

    public bool Remove(string name, out IDataElement? dataElement)
        => _elements.TryRemove(name, out dataElement);

    public TResult? GetOrAdd<TResult>(string name, Func<TResult> factory)
        where TResult : IDataElement
        => _elements.GetOrAdd(name, static (_, fac) => fac(), factory) is TResult res ? res : default;

    public void Init(string name, ISystemClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Name should not be Empty or null");
        Name = name;
        ModifyDate = clock.UtcNow.LocalDateTime;
        CreationDate = clock.UtcNow.LocalDateTime;
    }
        
    public override void Dispose()
    {
        base.Dispose();
            
        Name = string.Empty;
            
        foreach (var value in _elements.Values)
            value.Dispose();
            
        _elements.Clear();
    }

    public override void Recycle()
        => Dispose();
}