using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

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
        if(_elements.TryGetValue(name, out IDataElement? element))
        {
            element.Dispose();
            return (true, new DirectoryEntry(_elements.Remove(name)));
        }

        return (false, this);
    }

    [Pure]
    public (TResult Data, DirectoryEntry NewEntry) GetOrAdd<TResult>(string name, Func<TResult> factory)
        where TResult : class, IDataElement
    {
        if(_elements.TryGetValue(name, out IDataElement? element) && element is TResult result)
            return (result, this);

        result = factory();

        return (result, new DirectoryEntry(_elements.SetItem(name, result)));
    }

    [Pure]
    public (bool Added, DirectoryEntry NewEntry) TryAddElement(string name, IDataElement element) => 
        _elements.ContainsKey(Name) 
            ? (false, this) 
            : (true, new DirectoryEntry(_elements.Add(name, element)));

    public override void Dispose()
    {
        foreach (var element in _elements)
            element.Value.Dispose();
        
        GC.SuppressFinalize(this);
    }
}