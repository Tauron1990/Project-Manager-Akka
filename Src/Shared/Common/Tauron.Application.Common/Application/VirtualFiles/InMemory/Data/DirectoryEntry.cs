using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.PlatformServices;
using Stl.Fusion.Bridge.Internal;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public class DirectoryEntry : DataElementBase
{
    private readonly ImmutableDictionary<string, IDataElement> _elements;

    public IEnumerable<IDataElement> Elements => _elements.Values;

    public IEnumerable<FileEntry> Files => _elements.Values.OfType<FileEntry>();

    public IEnumerable<DirectoryEntry> Directorys => _elements.Values.OfType<DirectoryEntry>();

    public DirectoryEntry() => _elements = ImmutableDictionary<string, IDataElement>.Empty;

    private DirectoryEntry(ImmutableDictionary<string, IDataElement> elements)
    {
        _elements = elements;
    }
    
    [Pure]
    public (bool Removed, DirectoryEntry NewEntry) Remove(string name)
    {
        
    }

    public TResult GetOrAdd<TResult>(string name, Func<TResult> factory)
        where TResult : IDataElement
        => _elements.GetOrAdd(name, static (_, fac) => fac(), factory) is TResult res
            ? res
            : throw new InvalidCastException("Factory Created Wrong Type (Should Be Impossible)");

    public override void Dispose()
    {
        base.Dispose();

        Name = string.Empty;

        foreach (IDataElement value in _elements.Values)
            value.Dispose();

        _elements.Clear();
    }

    public bool TryAddElement(string name, IDataElement element)
        => _elements.TryAdd(name, element);
}