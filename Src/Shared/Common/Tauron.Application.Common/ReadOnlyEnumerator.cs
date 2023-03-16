using System.Collections;
using System.Collections.Generic;

namespace Tauron;

[PublicAPI]
public class ReadOnlyEnumerator<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _enumerable;

    public ReadOnlyEnumerator(IEnumerable<T> enumerable) => _enumerable = enumerable;

    public IEnumerator<T> GetEnumerator() => _enumerable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}