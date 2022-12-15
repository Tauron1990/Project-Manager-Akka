using System.Collections;

namespace Tauron.TextAdventure.Engine;

public sealed class SingleElementList<TValue> : IEnumerable<TValue>
{
    private readonly TValue _value;

    public SingleElementList(TValue value)
        => _value = value;

    public IEnumerator<TValue> GetEnumerator()
        => new Enumerator(_value);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    
    
    private class Enumerator : IEnumerator<TValue>
    {
        private bool _nextFinish;

        public Enumerator(TValue value)
            => Current = value;

        public bool MoveNext()
        {
            if(_nextFinish)
                return false;

            _nextFinish = true;

            return true;
        }

        public void Reset()
            => _nextFinish = false;

        public TValue Current { get; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

}