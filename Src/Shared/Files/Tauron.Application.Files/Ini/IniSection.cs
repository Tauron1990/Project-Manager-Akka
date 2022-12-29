using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Ini;

[PublicAPI]
[Serializable]
public sealed record IniSection(string Name, ImmutableDictionary<string, IniEntry> Entries) : IEnumerable<IniEntry>
{
    public IniSection(string name)
        : this(name, ImmutableDictionary<string, IniEntry>.Empty) { }

    public IEnumerator<IniEntry> GetEnumerator() => Entries.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public SingleIniEntry? GetSingleEntry(string name)
    {
        if(Entries.TryGetValue(name, out IniEntry? entry))
            return entry as SingleIniEntry;

        return null;
    }

    public IniSection AddSingleKey(string name)
    {
        var entry = new SingleIniEntry(name, null);

        return this with { Entries = Entries.Add(name, entry) };
    }

    public ListIniEntry? GetListEntry(string name)
    {
        if(!Entries.TryGetValue(name, out IniEntry? value)) return null;

        return value as ListIniEntry;
    }
}