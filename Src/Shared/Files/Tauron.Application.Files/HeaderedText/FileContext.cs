using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Application.Files.HeaderedText;

[PublicAPI]
public sealed class FileContext : IEnumerable<ContextEntry>
{
    internal FileContext(FileDescription description)
        => Description = (FileDescription)description.Clone();

    internal FileDescription Description { get; }

    internal List<ContextEntry> ContextEnries { get; } = new();

    public IEnumerable<ContextEntry> this[string key]
        => ContextEnries.Where(contextEnry => string.Equals(contextEnry.Key, key, System.StringComparison.Ordinal));

    public IEnumerator<ContextEntry> GetEnumerator() => ContextEnries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Reset()
        => ContextEnries.Clear();

    internal bool IsKeyword(string key) => Description.Contains(key);

    internal void Add(ContextEntry entry)
        => ContextEnries.Add(entry);
}