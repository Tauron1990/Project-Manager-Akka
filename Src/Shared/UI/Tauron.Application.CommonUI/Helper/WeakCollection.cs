﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Util.Internal;
using JetBrains.Annotations;
using Stl;

namespace Tauron.Application.CommonUI.Helper;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class WeakCollection<TType> : IList<Option<TType>>
    where TType : class
{
    private readonly Subject<Unit> _cleaned = new();
    private readonly List<WeakReference<TType>?> _internalCollection = new();

    public WeakCollection() => WeakCleanUp.RegisterAction(CleanUp);

    public int EffectiveCount
    {
        get
        {
            lock (_internalCollection)
            {
                return _internalCollection.Count(refer => refer?.IsAlive() ?? false);
            }
        }
    }

    public IObservable<Unit> WhenCleanedEvent => _cleaned.AsObservable();

    public Option<TType> this[int index]
    {
        get
        {
            lock (_internalCollection)
            {
                return _internalCollection[index]?.TypedTarget() ?? default;
            }
        }
        set
        {
            lock (_internalCollection)
            {
                _internalCollection[index] = value.HasValue ? new WeakReference<TType>(value.Value) : null;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count
    {
        get
        {
            lock (_internalCollection)
            {
                return _internalCollection.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public void Add(Option<TType> item)
    {
        if (!item.HasValue) return;

        lock (_internalCollection)
        {
            _internalCollection.Add(new WeakReference<TType>(item.Value));
        }
    }

    /// <summary>The clear.</summary>
    public void Clear()
    {
        lock (_internalCollection)
        {
            _internalCollection.Clear();
        }
    }

    public bool Contains(Option<TType> item)
    {
        lock (_internalCollection)
        {
            return item.HasValue && _internalCollection.Any(it => it?.TypedTarget() == item);
        }
    }

    public void CopyTo(Option<TType>[] array, int arrayIndex)
    {
        lock (_internalCollection)
        {
            var index = 0;
            for (var i = arrayIndex; i < array.Length; i++)
            {
                Option<TType> target = default;
                while (!target.HasValue && index <= _internalCollection.Count)
                {
                    target = _internalCollection[index]?.TypedTarget() ?? default;
                    index++;
                }

                if (!target.HasValue) break;

                array[i] = target;
            }
        }
    }

    public IEnumerator<Option<TType>> GetEnumerator()
    {
        lock (_internalCollection)
        {
            return
                _internalCollection
                   .ToArray()
                   .Select(reference => reference?.TypedTarget() ?? default)
                   .Where(target => target.HasValue)
                   .GetEnumerator();
        }
    }

    public int IndexOf(Option<TType> item)
    {
        lock (_internalCollection)
        {
            if (!item.HasValue) return -1;

            int index;
            for (index = 0; index < _internalCollection.Count; index++)
            {
                var temp = _internalCollection[index];

                if (temp?.TypedTarget() == item) break;
            }

            return index == _internalCollection.Count ? -1 : index;
        }
    }

    public void Insert(int index, Option<TType> item)
    {
        var (hasValue, value) = item;

        if (!hasValue) return;

        lock (_internalCollection)
        {
            _internalCollection.Insert(index, new WeakReference<TType>(value));
        }
    }

    public bool Remove(Option<TType> item)
    {
        if (!item.HasValue) return false;

        var index = IndexOf(item);

        if (index == -1) return false;

        lock (_internalCollection)
        {
            _internalCollection.RemoveAt(index);
        }

        return true;
    }

    public void RemoveAt(int index)
    {
        lock (_internalCollection)
        {
            _internalCollection.RemoveAt(index);
        }
    }

    internal void CleanUp()
    {
        lock (_internalCollection)
        {
            var dead = _internalCollection.Where(reference => !reference?.IsAlive() ?? false).ToArray();
            foreach (var genericWeakReference in dead) _internalCollection.Remove(genericWeakReference);
        }

        OnCleaned();
    }

    private void OnCleaned() => _cleaned.OnNext(Unit.Default);
}

[DebuggerNonUserCode]
[PublicAPI]
public class WeakReferenceCollection<TType> : Collection<TType>
    where TType : IInternalWeakReference
{
    private readonly object _gate = new();

    public WeakReferenceCollection() => WeakCleanUp.RegisterAction(CleanUpMethod);

    protected override void ClearItems()
    {
        lock (_gate)
        {
            base.ClearItems();
        }
    }

    protected override void InsertItem(int index, TType item)
    {
        lock (_gate)
        {
            if (index > Count) index = Count;
            base.InsertItem(index, item);
        }
    }

    protected override void RemoveItem(int index)
    {
        lock (_gate)
        {
            base.RemoveItem(index);
        }
    }

    protected override void SetItem(int index, TType item)
    {
        lock (_gate)
        {
            base.SetItem(index, item);
        }
    }

    private void CleanUpMethod()
    {
        lock (_gate)
        {
            Items.ToArray()
               .Where(it => !it.IsAlive)
               .ForEach(
                    it =>
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if (it is IDisposable dis) dis.Dispose();

                        Items.Remove(it);
                    });
        }
    }
}