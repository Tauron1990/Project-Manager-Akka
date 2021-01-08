using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using DynamicData.Binding;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public class UICollectionProperty<TData> : IReadOnlyList<TData>, INotifyCollectionChanged,
        INotifyPropertyChanged, INotifyCollectionChangedSuspender
    {
        private IObservableCollection<TData>? _collection;

        public UICollectionProperty(UIProperty<IObservableCollection<TData>> property)
        {
            Property = property;
            _collection = property.Value;
            property.Subscribe(c => _collection = c);
        }

        public UIProperty<IObservableCollection<TData>> Property { get; }

        public bool IsNull => _collection == null;
        //set
        //{
        //    if (_collection == null) return;
        //    _collection[index] = value;
        //}
        //int IReadOnlyCollection<TData>.Count => _collection?.Count ?? -1;

        //public void AddRange(IEnumerable<TData> datas)
        //{
        //    foreach (var data in datas) _collection?.Add(data);
        //}

        //public void RemoveRange(IEnumerable<TData> toRemove)
        //{
        //    foreach (var data in toRemove) _collection?.Remove(data);
        //}
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                if (_collection != null) _collection.CollectionChanged += value;
            }
            remove
            {
                if (_collection != null) _collection.CollectionChanged -= value;
            }
        }

        public IDisposable SuspendCount() => _collection?.SuspendCount() ?? Disposable.Empty;

        public IDisposable SuspendNotifications() => _collection?.SuspendNotifications() ?? Disposable.Empty;

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                if (_collection != null) _collection.PropertyChanged += value;
            }
            remove
            {
                if (_collection != null) _collection.PropertyChanged -= value;
            }
        }

        public int Count => _collection?.Count ?? -1;

        public IEnumerator<TData> GetEnumerator() => _collection?.GetEnumerator() ?? (IEnumerator<TData>) Array.Empty<TData>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //public void Add(TData item) => _collection?.Add(item);

        //public void Add(TData item, Action<int> then)
        //{
        //    if (_collection == null) return;
        //    _collection?.Add(item);
        //    then(Count);
        //}

        //public void Add(TData item, Action<TData> then)
        //{
        //    if(_collection == null) return;
        //    _collection.Add(item);
        //    then(item);
        //}

        //public void Clear() => _collection?.Clear();

        //public bool Contains(TData item) => _collection?.Contains(item) ?? false;

        //public void CopyTo(TData[] array, int arrayIndex) => _collection?.CopyTo(array, arrayIndex);

        //public bool Remove(TData item) => _collection?.Remove(item) ?? false;

        //int ICollection<TData>.Count => _collection?.Count ?? -1;

        //public bool IsReadOnly => _collection?.IsReadOnly ?? true;

        //public int IndexOf(TData item) => _collection?.IndexOf(item) ?? -1;

        //public void Insert(int index, TData item) => _collection?.Insert(index, item);

        //public void RemoveAt(int index) => _collection?.RemoveAt(index);

        public TData this[int index]
            => _collection != null ? _collection[index] : default!;
    }
}