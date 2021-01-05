﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Akka.Util.Internal;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Helper
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class WeakCollection<TType> : IList<TType>
        where TType : class
    {
        private readonly List<WeakReference<TType>?> _internalCollection = new();

        public WeakCollection()
        {
            WeakCleanUp.RegisterAction(CleanUp);
        }

        public int EffectiveCount
        {
            get
            {
                lock(_internalCollection)
                    return _internalCollection.Count(refer => refer?.IsAlive() ?? false);
            }
        }

        public TType? this[int index]
        {
            #pragma warning disable CS8613 // Die NULL-Zulässigkeit von Verweistypen im Rückgabetyp entspricht nicht dem implizit implementierten Member.
            #pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
            get
            {
                lock (_internalCollection)
                    return _internalCollection[index]?.TypedTarget();
            }
            #pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
            set
            {
                lock (_internalCollection)
                    _internalCollection[index] = value == null ? null : new WeakReference<TType>(value);
            }
            #pragma warning restore CS8613 // Die NULL-Zulässigkeit von Verweistypen im Rückgabetyp entspricht nicht dem implizit implementierten Member.
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                lock(_internalCollection)
                    return _internalCollection.Count;
            }
        }

        public bool IsReadOnly => false;

        public void Add(TType item)
        {
            if (item == null) return;
            lock(_internalCollection)
                _internalCollection.Add(new WeakReference<TType>(item));
        }

        /// <summary>The clear.</summary>
        public void Clear()
        {
            lock(_internalCollection)
                _internalCollection.Clear();
        }

        public bool Contains(TType item)
        {
            lock(_internalCollection)
                return item != null && _internalCollection.Any(it => it?.TypedTarget() == item);
        }

        public void CopyTo(TType[] array, int arrayIndex)
        {
            Argument.NotNull(array, nameof(array));

            lock (_internalCollection)
            {
                var index = 0;
                for (var i = arrayIndex; i < array.Length; i++)
                {
                    TType? target = null;
                    while (target == null && index <= _internalCollection.Count)
                    {
                        target = _internalCollection[index]?.TypedTarget();
                        index++;
                    }

                    if (target == null) break;

                    array[i] = target;
                }
            }
        }

        public IEnumerator<TType> GetEnumerator()
        {
            lock (_internalCollection)
            {
                return
                    _internalCollection
                       .ToArray()
                       .Select(reference => reference?.TypedTarget())
                       .Where(target => target != null)
                       .GetEnumerator()!;
            }
        }

        public int IndexOf(TType item)
        {
            lock (_internalCollection)
            {
                if (item == null) return -1;

                int index;
                for (index = 0; index < _internalCollection.Count; index++)
                {
                    var temp = _internalCollection[index];
                    if (temp?.TypedTarget() == item) break;
                }

                return index == _internalCollection.Count ? -1 : index;
            }
        }

        public void Insert(int index, TType item)
        {
            if (item == null) return;
            lock (_internalCollection)
                _internalCollection.Insert(index, new WeakReference<TType>(item));
        }

        public bool Remove(TType item)
        {
            if (item == null) return false;
            var index = IndexOf(item);
            if (index == -1) return false;

            lock (_internalCollection)
                _internalCollection.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            lock (_internalCollection)
                _internalCollection.RemoveAt(index);
        }

        public event EventHandler? CleanedEvent;

        internal void CleanUp()
        {
            lock (_internalCollection)
            {
                var dead = _internalCollection.Where(reference => !reference?.IsAlive() ?? false).ToArray();
                foreach (var genericWeakReference in dead) _internalCollection.Remove(genericWeakReference);
            }

            OnCleaned();
        }

        private void OnCleaned()
        {
            CleanedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    [DebuggerNonUserCode]
    [PublicAPI]
    public class WeakReferenceCollection<TType> : Collection<TType>
        where TType : IInternalWeakReference
    {
        private readonly object _gate = new();

        public WeakReferenceCollection() 
            => WeakCleanUp.RegisterAction(CleanUpMethod);

        protected override void ClearItems()
        {
            lock (_gate) 
                base.ClearItems();
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
                base.RemoveItem(index);
        }

        protected override void SetItem(int index, TType item)
        {
            lock (_gate) 
                base.SetItem(index, item);
        }

        private void CleanUpMethod()
        {
            lock (_gate)
            {
                Items.ToArray()
                    .Where(it => !it.IsAlive)
                    .ForEach(it =>
                    {
                        if (it is IDisposable dis) dis.Dispose();

                        Items.Remove(it);
                    });
            }
        }
    }
}