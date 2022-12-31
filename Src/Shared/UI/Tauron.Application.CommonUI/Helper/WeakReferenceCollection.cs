using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Akka.Util.Internal;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Helper;

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
            if(index > Count) index = Count;
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
                        if(it is IDisposable dis) dis.Dispose();

                        Items.Remove(it);
                    });
        }
    }
}