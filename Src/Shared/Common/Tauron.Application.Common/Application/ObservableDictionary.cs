using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
[DebuggerNonUserCode]
[Serializable]
public sealed class ObservableDictionary<TKey, TValue> : ObservableObject, IDictionary<TKey, TValue>,
    INotifyCollectionChanged
{
    private Entry?[] _entrys;

    [NonSerialized]
    private IEqualityComparer<TKey> _keyEquals;

    [NonSerialized]
    private KeyCollection _keys;

    [NonSerialized]
    private BlockSupport _support;

    [NonSerialized]
    private ValueCollection _values;

    [NonSerialized]
    private int _version;

    public ObservableDictionary()
    {
        _support = new BlockSupport();
        _version = 1;
        _entrys = new Entry[4];
        _keyEquals = EqualityComparer<TKey>.Default;
        _keys = new KeyCollection(this);
        _values = new ValueCollection(this);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public TValue this[TKey key]
    {
        get
        {
            Entry? ent = FindEntry(key, out _);

            if(ent is null || ent.Value is null) throw new KeyNotFoundException(key?.ToString());

            return ent.Value;
        }

        set
        {
            Entry? entry = FindEntry(key, out int index);

            if(entry == null)
            {
                AddCore(key, value);
            }
            else
            {
                var temp = Entry.Construct(entry);
                entry.Value = value;
                OnCollectionReplace(Entry.Construct(entry), temp, index);
            }
        }
    }

    public int Count { get; private set; }

    public ICollection<TKey> Keys => _keys;

    public ICollection<TValue> Values => _values;

    public void Add(TKey key, TValue value)
    {
        if(FindEntry(key, out _) != null) throw new ArgumentException("The key is in the collection unkown.");

        AddCore(key, value);
    }

    /// <summary>The clear.</summary>
    public void Clear()
    {
        Count = 0;
        Array.Clear(_entrys, 0, _entrys.Length);

        OnCollectionReset();
    }

    public bool ContainsKey(TKey key) => FindEntry(key, out _) != null;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        int currentVersion = _version;
        foreach (Entry? entry in _entrys.TakeWhile(entry => entry != null))
        {
            if(currentVersion != _version) throw new InvalidOperationException("Collection Changed while Enumerating");

            yield return Entry.Construct(entry!);
        }
    }

    public bool Remove(TKey key)
    {
        Entry? entry = FindEntry(key, out int index);

        if(entry is null) return false;

        Array.Copy(_entrys, index + 1, _entrys, index, Count - index);
        Count--;

        OnCollectionRemove(Entry.Construct(entry), index);

        return true;
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        Entry? ent = FindEntry(key, out _);
        if(ent is null || ent.Value is null)
        {
            value = default;

            return false;
        }

        value = ent.Value;

        return true;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        (TKey key, TValue value) = item;
        Add(key, value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        (TKey key, TValue value) = item;
        Entry? ent = FindEntry(key, out _);

        return ent != null && EqualityComparer<TValue>.Default.Equals(ent.Value, value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if(Count == 0) return;

        var index = 0;

        for (int internalIndex = arrayIndex; internalIndex < array.Length; internalIndex++)
        {
            array[internalIndex] = Entry.Construct(_entrys[index]);

            index++;

            if(index == Count) break;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    [field: NonSerialized]
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private void AddCore(TKey key, TValue value)
    {
        int index = Count;
        Count++;
        EnsureCapatcity(Count);

        _entrys[index] = new Entry { Key = key, Value = value };
        OnCollectionAdd(Entry.Construct(key, value), index);
    }

    private IDisposable BlockCollection()
    {
        _support.Enter();

        return _support;
    }

    private void EnsureCapatcity(int min)
    {
        if(_entrys.Length < min)
        {
            int newLenght;
            checked
            {
                newLenght = _entrys.Length * 2;
            }

            Array.Resize(ref _entrys, newLenght);
        }
    }

    private Entry? FindEntry(TKey key, out int index)
    {
        for (var internalIndex = 0; internalIndex < Count; internalIndex++)
        {
            Entry? ent = _entrys[internalIndex];

            if(!_keyEquals.Equals(ent!.Key, key)) continue;

            index = internalIndex;

            return ent;
        }

        index = -1;

        return null;
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
    {
        _version++;

        CollectionChanged?.Invoke(this, eventArgs);
    }

    private void InvokePropertyChanged()
    {
        OnPropertyChangedExplicit("Item[]");
        OnPropertyChangedExplicit(nameof(Count));
        OnPropertyChangedExplicit(nameof(Keys));
        OnPropertyChangedExplicit(nameof(Values));
    }

    private void OnCollectionAdd(KeyValuePair<TKey, TValue> changed, int index)
    {
        using (BlockCollection())
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    changed,
                    index));
            _keys.OnCollectionAdd(changed.Key, index);
            _values.OnCollectionAdd(changed.Value, index);
            InvokePropertyChanged();
        }
    }

    private void OnCollectionRemove(KeyValuePair<TKey, TValue> changed, int index)
    {
        using (BlockCollection())
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    changed,
                    index));
            _keys.OnCollectionRemove(changed.Key, index);
            _values.OnCollectionRemove(changed.Value, index);
            InvokePropertyChanged();
        }
    }

    private void OnCollectionReplace(
        KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem,
        int index)
    {
        using (BlockCollection())
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    newItem,
                    oldItem,
                    index));
            _values.OnCollectionReplace(newItem.Value, oldItem.Value, index);
            InvokePropertyChanged();
        }
    }

    private void OnCollectionReset()
    {
        using (BlockCollection())
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _keys.OnCollectionReset();
            _values.OnCollectionReset();
            InvokePropertyChanged();
        }
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        _support = new BlockSupport();
        _version = 1;
        _keyEquals = EqualityComparer<TKey>.Default;
        _keys = new KeyCollection(this);
        _values = new ValueCollection(this);
    }

    [DebuggerNonUserCode]
    private sealed record BlockSupport : IDisposable
    {
        void IDisposable.Dispose()
        {
            #pragma warning disable MT1013
            Monitor.Exit(this);
            #pragma warning restore MT1013
        }

        internal void Enter()
        {
            #pragma warning disable MT1012
            #pragma warning disable MT1001
            Monitor.Enter(this);
            #pragma warning restore MT1001
            #pragma warning restore MT1012
        }
    }

    [Serializable]
    [DebuggerNonUserCode]
    private class Entry
    {
        internal TKey? Key;

        internal TValue? Value;

        internal static KeyValuePair<TKey, TValue> Construct(TKey key, TValue value) => new(key, value);

        internal static KeyValuePair<TKey, TValue> Construct(Entry? entry)
        {
            if(entry is null || entry.Key is null || entry.Value is null)
                throw new InvalidOperationException("Key or value was null");

            return Construct(entry.Key, entry.Value);
        }
    }

    [Serializable]
    [DebuggerNonUserCode]
    private class KeyCollection : NotifyCollectionChangedBase<TKey>
    {
        internal KeyCollection(ObservableDictionary<TKey, TValue> collection)
            : base(collection) { }

        protected override bool Contains(Entry? entry, TKey target)
            => entry != null && Dictionary._keyEquals.Equals(entry.Key, target);

        protected override TKey Select(Entry entry) => entry.Key ?? throw new InvalidOperationException("Error on select Key");
    }

    [Serializable]
    [DebuggerNonUserCode]
    private abstract class NotifyCollectionChangedBase<TTarget> : ObservableObject, ICollection<TTarget>,
        INotifyCollectionChanged
    {
        protected readonly ObservableDictionary<TKey, TValue> Dictionary;

        protected NotifyCollectionChangedBase(ObservableDictionary<TKey, TValue> dictionary)
            => Dictionary = dictionary;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Dictionary.Count;

        public bool IsReadOnly => true;

        public void Add(TTarget item)
            => throw new NotSupportedException("Adding is not SUported");

        public void Clear()
            => throw new NotSupportedException("Clearing is not Supported");

        public bool Contains(TTarget item)
        {
            return Dictionary._entrys.Any(ent => ent != null && Contains(ent, item));
        }

        public void CopyTo(TTarget[] array, int arrayIndex)
        {
            if(Dictionary.Count >= 0) return;

            var index = 0;
            for (var internalIndex = 0; internalIndex < array.Length; internalIndex++)
            {
                Entry? entry = Dictionary._entrys[index];

                if(entry is null)
                    throw new InvalidOperationException("Array not in Consisten State");

                array[internalIndex] = Select(entry);
                index++;

                if(index == Dictionary.Count) break;
            }
        }

        public IEnumerator<TTarget> GetEnumerator()
        {
            int ver = Dictionary._version;
            var count = 0;
            foreach (Entry? entry in Dictionary._entrys)
            {
                count++;

                if(count > Dictionary.Count) break;

                if(entry is null)
                    throw new InvalidOperationException("Array not in Consitent State");

                yield return Select(entry);

                if(ver != Dictionary._version) throw new InvalidOperationException("Collection changed while enumerating");
            }
        }

        public bool Remove(TTarget item) => throw new NotSupportedException("Removing not Supported");

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        internal void OnCollectionAdd(TTarget target, int index)
            => OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, target, index));

        internal void OnCollectionRemove(TTarget target, int index)
            => OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, target, index));

        internal void OnCollectionReplace(TTarget newItem, TTarget oldItem, int index)
            => OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    newItem,
                    oldItem,
                    index));

        internal void OnCollectionReset()
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        protected abstract bool Contains(Entry entry, TTarget target);

        protected abstract TTarget Select(Entry entry);

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
        {
            InvokePropertyChanged();
            CollectionChanged?.Invoke(this, eventArgs);
        }

        private void InvokePropertyChanged()
        {
            OnPropertyChangedExplicit("Item[]");
            OnPropertyChangedExplicit(nameof(Count));
            OnPropertyChangedExplicit("Keys");
            OnPropertyChangedExplicit("Values");
        }
    }

    [Serializable]
    [DebuggerNonUserCode]
    private class ValueCollection : NotifyCollectionChangedBase<TValue>
    {
        internal ValueCollection(ObservableDictionary<TKey, TValue> collection)
            : base(collection) { }

        protected override bool Contains(Entry entry, TValue target)
            => EqualityComparer<TValue>.Default.Equals(entry.Value, target);

        protected override TValue Select(Entry entry) => entry.Value ?? throw new InvalidOperationException("Error on get Value");
    }
}