﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class FluentCollectionPropertyRegistration<TData>
    {
        private readonly IUiActor                     _actor;
        private          ObservableCollection<TData> _collection = new();
        private          bool                        _isAsync;

        internal FluentCollectionPropertyRegistration(string name, IUiActor actor)
        {
            _actor   = actor;
            Property = new UIProperty<ObservableCollection<TData>>(name);
            Property.Set(_collection.ToMaybe()!);
            actor.RegisterProperty(Property);
        }

        public UIProperty<ObservableCollection<TData>> Property { get; }

        public FluentCollectionPropertyRegistration<TData> AndAsync(bool viaCollectionItSelf = false)
        {
            if (_isAsync) return this;
            _isAsync = true;

            if (viaCollectionItSelf)
            {
                _collection = new UIObservableCollection<TData>(_collection);
                Property.Set(_collection.ToMaybe()!);
            }
            else
            {
                _actor.Dispatcher.Invoke(() => BindingOperations.EnableCollectionSynchronization(_collection, _actor));

                _actor.RegisterTerminationCallback(a => a.Dispatcher.Invoke(() => BindingOperations.DisableCollectionSynchronization(_collection)));
            }

            Property.LockSet();

            return this;
        }

        public FluentCollectionPropertyRegistration<TData> AndInitialElements(params TData[] elements)
            => AndInitialElements((IEnumerable<TData>) elements);

        public FluentCollectionPropertyRegistration<TData> AndInitialElements(IEnumerable<TData> elements)
        {
            foreach (var element in elements)
                _collection.Add(element);

            return this;
        }

        public FluentCollectionPropertyRegistration<TData> AndConfigurateSource(Action<ICollectionView> view)
        {
            view(CollectionViewSource.GetDefaultView(_collection));
            return this;
        }

        public static implicit operator UICollectionProperty<TData>(FluentCollectionPropertyRegistration<TData> config) => new(config.Property);
    }
}