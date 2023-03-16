using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Tauron.Application;

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
            if(index > Count) index = Count;
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
                        if(it is IDisposable dis) dis.Dispose();

                        Items.Remove(it);
                    });
        }
    }
}