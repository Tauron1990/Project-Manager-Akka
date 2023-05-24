using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.PlatformServices;
using Tauron.Errors;
using Tauron.ObservableExt;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public class DirectoryEntry : DataElementBase
{
    private readonly Dictionary<string, IDataElement> _elements = new(StringComparer.Ordinal);

    public IEnumerable<IDataElement> Elements => _elements.Values;

    public IEnumerable<FileEntry> Files => _elements.Values.OfType<FileEntry>();

    public IEnumerable<DirectoryEntry> Directorys => _elements.Values.OfType<DirectoryEntry>();

    public bool Remove(string name)
    {
        lock(_elements)
            return _elements.Remove(name);
    }

    public Result<TResult> GetOrAdd<TResult>(string name, Func<Result<TResult>> factory)
        where TResult : IDataElement
    {
        lock (_elements)
        {
            if(_elements.TryGetValue(name, out var element))
            {
                if(element is TResult data) return data;

                return new TypeMismatch().CausedBy("Duplicate Element with wrong Type");
            }

            return
                from data in factory()
                from add in Result.Try(() => _elements.Add(name, data)).ToUnit()
                select Result.Ok(data);
        }
    }

    public Result<DirectoryEntry> Init(string name, ISystemClock clock)
    {
        if(string.IsNullOrWhiteSpace(name))
            return new InvalidOperation().CausedBy("Name should not be Empty or null");

        Name = name;
        ModifyDate = clock.UtcNow.LocalDateTime;
        
        return this;
    }

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