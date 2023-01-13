using System.Collections;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.Data;

[PublicAPI]
public sealed class StateList<TType> : IEnumerable<TType>
{
    private ImmutableDictionary<string, ImmutableList<TType>> _list = ImmutableDictionary<string, ImmutableList<TType>>.Empty;

    public IEnumerator<TType> GetEnumerator()
        => _list.SelectMany(p => p.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public void Set(string name)
        => _list = _list.SetItem(name, ImmutableList<TType>.Empty);

    public void Set(string name, TType element)
        => _list = _list.SetItem(name, ImmutableList<TType>.Empty.Add(element));

    public void Set(string name, IEnumerable<TType> element)
        => _list = _list.SetItem(name, ImmutableList<TType>.Empty.AddRange(element));
}