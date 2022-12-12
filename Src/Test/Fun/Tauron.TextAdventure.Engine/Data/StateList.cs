using System.Collections;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.Data;

[PublicAPI]
public sealed class StateList<TType> : IEnumerable<TType>
{
    private ImmutableList<TType> _list = ImmutableList<TType>.Empty;

    public void Append(TType element)
        => _list = _list.Add(element);

    public void Clear()
        => _list = ImmutableList<TType>.Empty;

    public IEnumerator<TType> GetEnumerator()
        => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)_list).GetEnumerator();
}