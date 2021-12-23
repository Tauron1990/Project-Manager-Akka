using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Stl;

namespace Tauron.Application;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class WeakCollection<TType> : IList<Option<TType>>
    where TType : class
{
    private readonly Subject<Unit> _cleaned = new();
    private readonly List<WeakReference<TType>> _internalCollection = new();

    public WeakCollection()
    {
        WeakCleanUp.RegisterAction(CleanUp);
    }

    public int EffectiveCount => _internalCollection.Count(refer => refer.IsAlive());
    public IObservable<Unit> WhenCleanedEvent => _cleaned.AsObservable();

    public Option<TType> this[int index]
    {
        get => _internalCollection[index].TypedTarget();
        set => value.OnSuccess(value => _internalCollection[index] = new WeakReference<TType>(value));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _internalCollection.Count;

    public bool IsReadOnly => false;

    public void Add(Option<TType> item)
        => item.OnSuccess(target => _internalCollection.Add(new WeakReference<TType>(target)));

    /// <summary>The clear.</summary>
    public void Clear() => _internalCollection.Clear();

    public bool Contains(Option<TType> item)
        => item.HasValue && _internalCollection.Any(it => it.TypedTarget() == item);

    public void CopyTo(Option<TType>[] array, int arrayIndex)
    {
        var index = 0;
        for (var targetIndex = arrayIndex; targetIndex < array.Length; targetIndex++)
        {
            var target = Option<TType>.None;
            while (!target.HasValue && index <= _internalCollection.Count)
            {
                target = _internalCollection[index].TypedTarget();
                index++;
            }

            if (!target.HasValue) break;

            array[targetIndex] = target;
        }
    }

    public IEnumerator<Option<TType>> GetEnumerator()
        => _internalCollection.Select(reference => reference.TypedTarget())
           .GetEnumerator();

    public int IndexOf(Option<TType> item)
    {
        if (!item.HasValue) return -1;

        int index;
        for (index = 0; index < _internalCollection.Count; index++)
        {
            var temp = _internalCollection[index];

            if (temp.TypedTarget() == item) break;
        }

        return index == _internalCollection.Count ? -1 : index;
    }

    public void Insert(int index, Option<TType> item)
    {
        if (!item.HasValue) return;

        _internalCollection.Insert(index, new WeakReference<TType>(item.Value));
    }

    public bool Remove(Option<TType> item)
    {
        if (!item.HasValue) return false;

        var index = IndexOf(item);

        if (index == -1) return false;

        _internalCollection.RemoveAt(index);

        return true;
    }

    public void RemoveAt(int index) => _internalCollection.RemoveAt(index);

    internal void CleanUp()
    {
        var dead = _internalCollection.Where(reference => !reference.IsAlive()).ToArray();
        foreach (var genericWeakReference in dead) _internalCollection.Remove(genericWeakReference);

        OnCleaned();
    }

    private void OnCleaned() => _cleaned.OnNext(Unit.Default);
}

[DebuggerNonUserCode]
[PublicAPI]
public class WeakReferenceCollection<TType> : Collection<TType>
    where TType : IWeakReference
{
    private readonly object _lock = new();

    public WeakReferenceCollection()
    {
        WeakCleanUp.RegisterAction(CleanUpMethod);
    }

    protected override void ClearItems()
    {
        lock (_lock)
        {
            base.ClearItems();
        }
    }

    protected override void InsertItem(int index, TType item)
    {
        lock (_lock)
        {
            if (index > Count) index = Count;
            base.InsertItem(index, item);
        }
    }

    protected override void RemoveItem(int index)
    {
        lock (_lock)
        {
            base.RemoveItem(index);
        }
    }

    protected override void SetItem(int index, TType item)
    {
        lock (_lock)
        {
            base.SetItem(index, item);
        }
    }

    private void CleanUpMethod()
    {
        lock (_lock)
        {
            Items.ToArray()
               .Where(it => !it.IsAlive)
               .ToArray()
               .Foreach(
                    it =>
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if (it is IDisposable dis) dis.Dispose();

                        Items.Remove(it);
                    });
        }
    }
}