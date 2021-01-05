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
        private ObservableCollectionExtended<TData> _collection = new();

        internal FluentCollectionPropertyRegistration(string name, UiActor actor)
        {
            _actor = actor;
            Property = new UIProperty<IObservableCollection<TData>>(name);
            Property.Set(_collection);
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

        public IDisposable BindTo(IObservable<IChangeSet<TData>> source, Func<IObservable<IChangeSet<TData>>, IDisposable>? subscriber = null)
        {
            subscriber ??= set => set.Subscribe();

            return subscriber(source
                             .ObserveOn(DispatcherScheduler.From(_actor.Dispatcher))
                             .Bind(_collection));
        }
        
        public static implicit operator UICollectionProperty<TData>(FluentCollectionPropertyRegistration<TData> config) => new(config.Property);
    }
}