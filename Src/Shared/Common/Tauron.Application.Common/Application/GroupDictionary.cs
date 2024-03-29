using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Tauron.Application;

[Serializable]
[PublicAPI]
[DebuggerStepThrough]
public class GroupDictionary<TKey, TValue> : Dictionary<TKey, ICollection<TValue>>
    where TKey : notnull
{
    private readonly Type _listType;

    public GroupDictionary(Type listType) => _listType = listType;

    public GroupDictionary() => _listType = typeof(List<TValue>);

    #pragma warning disable AV1564
    public GroupDictionary(bool singleList)
        #pragma warning restore AV1564
        => _listType = singleList ? typeof(HashSet<TValue>) : typeof(List<TValue>);

    public GroupDictionary(GroupDictionary<TKey, TValue> groupDictionary)
        : base(groupDictionary)
        => _listType = groupDictionary._listType;

    protected GroupDictionary(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        if(info.GetValue("listType", typeof(Type)) is not Type listType)
            throw new InvalidOperationException("List Type not in Serialization info");

        _listType = listType;
    }

    public ICollection<TValue> AllValues => new AllValueCollection(this);

    #pragma warning disable AV1010
    public new ICollection<TValue> this[TKey key]
        #pragma warning restore AV1010
    {
        get
        {
            if(!ContainsKey(key)) Add(key);

            return base[key];
        }

        set => base[key] = value;
    }

    private object CreateList()
    {
        if(!typeof(ICollection<TValue>).IsAssignableFrom(_listType))
            throw new InvalidOperationException("List Type not Compatible With GroupDicitioonary Value Type");

        Type genericTemp;

        if(_listType.ContainsGenericParameters)
        {
            if(_listType.GetGenericArguments().Length != 1)
                throw new InvalidOperationException("More then one Genric Type Parameter in Provided List Type");

            genericTemp = _listType.MakeGenericType(typeof(TValue));
        }
        else
        {
            if(!_listType.IsGenericType)
            {
                genericTemp = _listType;
            }
            else
            {
                var generic = _listType.GetGenericArguments();

                genericTemp = generic.Length == 0 ? _listType : _listType.GetGenericTypeDefinition().MakeGenericType(typeof(TValue));
            }
        }

        if(genericTemp is null) throw new InvalidOperationException("List Type for Group Dicitionay not Successful Created");

        return Activator.CreateInstance(genericTemp) ??
               throw new InvalidOperationException("List Creation Failed");
    }

    public void AddRange(TKey key, IEnumerable<TValue> value)
    {
        if(!ContainsKey(key)) Add(key);

        var values = base[key];

        // ReSharper disable once PossibleMultipleEnumeration
        foreach (TValue item in value.Where(item => item != null)) values.Add(item);
    }


    public bool RemoveValue(TValue value) => RemoveImpl(default!, value, false, true);

    private bool RemoveImpl(TKey key, TValue val, bool removeEmpty, bool removeAll)
        => removeAll ? RemoveAll(val, removeEmpty) : RemoveSingleObject(key, val, removeAll);

    private bool RemoveAll(TValue val, bool removeEmpty)
    {
        var ok = false;
        IEnumerator keys = Keys.ToArray().GetEnumerator();
        IEnumerator vals = Values.ToArray().GetEnumerator();
        while (keys.MoveNext() && vals.MoveNext())
        {
            var coll = vals.Current as ICollection<TValue> ?? Array.Empty<TValue>();

            if(keys.Current is not TKey currkey)
                throw new InvalidCastException("Provided Key is not Right Type");

            ok |= RemoveList(coll, val);

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once AssignNullToNotNullAttribute
            if(removeEmpty && coll.Count == 0) ok |= Remove(currkey);
        }

        return ok;
    }

    private bool RemoveSingleObject(TKey key, TValue val, bool removeEmpty)
    {
        bool ok = ContainsKey(key);

        if(!ok) return false;

        var col = base[key];

        ok |= RemoveList(col, val);

        if(!removeEmpty) return true;

        if(col.Count == 0) ok |= Remove(key);

        return ok;
    }

    private static bool RemoveList(ICollection<TValue> vals, TValue val)
    {
        var ok = false;
        while (vals.Remove(val)) ok = true;

        return ok;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("listType", _listType, typeof(Type));

        base.GetObjectData(info, context);
    }

    private class AllValueCollection : ICollection<TValue>
    {
        private readonly GroupDictionary<TKey, TValue> _list;

        internal AllValueCollection(GroupDictionary<TKey, TValue> list)
            => _list = list;

        private IEnumerable<TValue> GetAll => _list.SelectMany(pair => pair.Value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => GetAll.Count();

        public bool IsReadOnly => true;

        public void Add(TValue item)
            => throw new NotSupportedException("Item Adding ist not Supported");

        public void Clear()
            => throw new NotSupportedException("All Values Collection can not Cleared");

        public bool Contains(TValue item) => GetAll.Contains(item);

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            GetAll.ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<TValue> GetEnumerator() => GetAll.GetEnumerator();

        public bool Remove(TValue item) => throw new NotSupportedException("Item Removing is not Supported");
    }

    #pragma warning disable AV1551
    public void Add(TKey key)
    {
        if(!ContainsKey(key)) base[key] = (ICollection<TValue>)CreateList();
    }

    public void Add(TKey key, TValue value)
    {
        if(!ContainsKey(key)) Add(key);

        var list = base[key];
        list.Add(value);
    }
    #pragma warning restore AV1551

    #pragma warning disable AV1564
    #pragma warning disable AV1551
    public bool Remove(TValue value, bool removeEmptyLists) => RemoveImpl(default!, value, removeEmptyLists, true);
    #pragma warning restore AV1564

    public bool Remove(TKey key, TValue value) => RemoveImpl(key, value, false, false);

    #pragma warning disable AV1564
    public bool Remove(TKey key, TValue value, bool removeListIfEmpty)
        #pragma warning restore AV1564
        => RemoveImpl(key, value, removeListIfEmpty, false);
    #pragma warning restore AV1551
}