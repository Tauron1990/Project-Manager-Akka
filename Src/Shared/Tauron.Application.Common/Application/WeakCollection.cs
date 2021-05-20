using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Util;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class WeakCollection<TType> : IList<Option<TType>>
        where TType : class
    {
        private readonly List<WeakReference<TType>> _internalCollection = new();

        public WeakCollection()
        {
            WeakCleanUp.RegisterAction(CleanUp);
        }

        public int EffectiveCount => _internalCollection.Count(refer => refer.IsAlive());

        public Option<TType> this[int index]
        {
            get => _internalCollection[index].TypedTarget();
            set => value.OnSuccess(value => _internalCollection[index] = new WeakReference<TType>(value));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _internalCollection.Count;

        public bool IsReadOnly => false;

        public void Add(Option<TType> item) 
            => item.OnSuccess(i => _internalCollection.Add(new WeakReference<TType>(i)));

        /// <summary>The clear.</summary>
        public void Clear() => _internalCollection.Clear();

        public bool Contains(Option<TType> item) 
            => item.HasValue && _internalCollection.Any(it => it.TypedTarget() == item);

        public void CopyTo(Option<TType>[] array, int arrayIndex)
        {
            Argument.NotNull(array, nameof(array));

            var index = 0;
            for (var i = arrayIndex; i < array.Length; i++)
            {
                Option<TType> target = Option<TType>.None;
                while (target.IsEmpty && index <= _internalCollection.Count)
                {
                    target = _internalCollection[index].TypedTarget();
                    index++;
                }

                if (target.IsEmpty) break;

                array[i] = target;
            }
        }

        public IEnumerator<Option<TType>> GetEnumerator()
        {
            return
                _internalCollection.Select(reference => reference.TypedTarget())
                    .Where(target => target.HasValue)
                    .GetEnumerator()!;
        }

        public int IndexOf(Option<TType> item)
        {
            if (item.IsEmpty) return -1;

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
            if (item.IsEmpty) return;
            _internalCollection.Insert(index, new WeakReference<TType>(item.Value));
        }

        public bool Remove(Option<TType> item)
        {
            if (item.IsEmpty) return false;
            var index = IndexOf(item);
            if (index == -1) return false;

            _internalCollection.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index) => _internalCollection.RemoveAt(index);

        private readonly Subject<Unit> _cleaned = new();
        public IObservable<Unit> WhenCleanedEvent => _cleaned.AsObservable();

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
        public WeakReferenceCollection()
        {
            WeakCleanUp.RegisterAction(CleanUpMethod);
        }

        protected override void ClearItems()
        {
            lock (this)
            {
                base.ClearItems();
            }
        }

        protected override void InsertItem(int index, TType item)
        {
            lock (this)
            {
                if (index > Count) index = Count;
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (this)
            {
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, TType item)
        {
            lock (this)
            {
                base.SetItem(index, item);
            }
        }

        private void CleanUpMethod()
        {
            lock (this)
            {
                Items.ToArray()
                    .Where(it => !it.IsAlive)
                    .ToArray()
                    .Foreach(
                        it =>
                        {
                            if (it is IDisposable dis) dis.Dispose();

                            Items.Remove(it);
                        });
            }
        }
    }
}