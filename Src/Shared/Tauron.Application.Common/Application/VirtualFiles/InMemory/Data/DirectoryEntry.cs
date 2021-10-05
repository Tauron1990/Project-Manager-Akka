using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MHLab.Pooling;

namespace Tauron.Application.VirtualFiles.InMemory.Data
{ 
    public class DirectoryEntry : IDataElement
    {
        private readonly ConcurrentDictionary<string, IDataElement> _elements = new();

        public IEnumerable<FileEntry> Files => _elements.Values.OfType<FileEntry>();

        public IEnumerable<DirectoryEntry> Directorys => _elements.Values.OfType<DirectoryEntry>();

        public bool Remove(string name, out IDataElement? dataElement)
            => _elements.TryRemove(name, out dataElement);

        public TResult? GetOrAdd<TResult>(string name, Func<TResult> factory)
            where TResult : IDataElement
            => _elements.GetOrAdd(name, static (_, fac) => fac(), factory) is TResult res ? res : default;
        
        public virtual void Dispose()
        {
            foreach (var value in _elements.Values)
                value.Dispose();
            
            _elements.Clear();
        }

        void IPoolable.Recycle()
            => Dispose();
    }
}