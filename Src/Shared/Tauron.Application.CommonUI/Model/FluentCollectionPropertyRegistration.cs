using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.AppCore;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public sealed class FluentCollectionPropertyRegistration<TData>
    {
        private readonly UiActor _actor;
        private readonly ObservableCollectionExtended<TData> _collection = new();

        internal FluentCollectionPropertyRegistration(string name, UiActor actor)
        {
            _actor = actor;
            Property = new UIProperty<IObservableCollection<TData>>(name);
            Property.Set(_collection);
            Property.LockSet();
            actor.RegisterProperty(Property);
        }

        public UIProperty<IObservableCollection<TData>> Property { get; }

        public FluentCollectionPropertyRegistration<TData> AndInitialElements(params TData[] elements)
            => AndInitialElements((IEnumerable<TData>) elements);

        public FluentCollectionPropertyRegistration<TData> AndInitialElements(IEnumerable<TData> elements)
        {
            foreach (var element in elements)
                _collection.Add(element);

            return this;
        }

        public UICollectionProperty<TData> BindTo(IObservable<IChangeSet<TData>> source,
            Func<IObservable<IChangeSet<TData>>, IDisposable>? subscriber = null)
        {
            subscriber ??= set => set.Subscribe();

            subscriber(source
                    .ObserveOn(DispatcherScheduler.From(_actor.Dispatcher))
                    .Bind(_collection))
                .DisposeWith(_actor);

            return this;
        }

        public UICollectionProperty<TData> BindTo<TKey>(IObservable<IChangeSet<TData, TKey>> source,
            Func<IObservable<IChangeSet<TData, TKey>>, IDisposable>? subscriber = null)
            where TKey : notnull
        {
            subscriber ??= set => set.Subscribe();

            subscriber(source
                    .ObserveOn(DispatcherScheduler.From(_actor.Dispatcher))
                    .Bind(_collection))
                .DisposeWith(_actor);

            return this;
        }

        public UICollectionProperty<TData> BindToCache<TKey>(Func<TData, TKey> keySelector,
            out SourceCache<TData, TKey> sourceCollection)
            where TKey : notnull
        {
            sourceCollection = new SourceCache<TData, TKey>(keySelector);

            return BindTo(sourceCollection.Connect());
        }

        public UICollectionProperty<TData> BindToList(out SourceList<TData> sourceCollection)
        {
            sourceCollection = new SourceList<TData>();

            return BindTo(sourceCollection.Connect());
        }

        public UICollectionProperty<TData> BindToList(IEnumerable<TData> initial,  out SourceList<TData> sourceCollection)
        {
            BindToList(out sourceCollection);

            sourceCollection.AddRange(initial);

            return this;
        }

        public static implicit operator UICollectionProperty<TData>(FluentCollectionPropertyRegistration<TData> config)
            => new(config.Property);
    }
}