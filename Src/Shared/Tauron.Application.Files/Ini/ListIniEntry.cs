using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Tauron.Application.Files.Ini;

[Serializable]
public sealed record ListIniEntry(string Key, ImmutableList<string> Values) : IniEntry(Key)
{
    public ListIniEntry(SingleIniEntry entry)
        : this(entry.Key, ImmutableList<string>.Empty.Add(entry.Value ?? string.Empty)) { }

    public ListIniEntry(string key, IEnumerable<string> values) : this(key, ImmutableList<string>.Empty.AddRange(values)) { }
}