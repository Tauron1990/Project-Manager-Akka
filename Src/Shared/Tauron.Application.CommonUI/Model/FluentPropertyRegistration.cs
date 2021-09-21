using System;
using System.Reactive;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public sealed class FluentPropertyRegistration<TData>
    {
        internal FluentPropertyRegistration(string name, UiActor actor)
        {
            Actor = actor;
            Property = new UIProperty<TData>(name);
            actor.RegisterProperty(Property);
        }

        public UIProperty<TData> Property { get; }

        public UiActor Actor { get; }

        public FluentPropertyRegistration<TData> WithValidator(Func<IObservable<TData>, IObservable<Error?>> validator)
        {
            Property.SetValidator(validator);

            return this;
        }

        public FluentPropertyRegistration<TData> WithDefaultValue(TData data)
        {
            Property.Set(data);

            return this;
        }

        public FluentPropertyRegistration<TData> OnChange(Action changed)
        {
            Property.Subscribe(_ => changed()).DisposeWith(Actor);

            return this;
        }

        public FluentPropertyRegistration<TData> OnChange(Action<TData> changed)
        {
            Property.Subscribe(changed).DisposeWith(Actor);

            return this;
        }

        public FluentPropertyRegistration<TData> OnChange(Action<IObservable<Unit>> changed)
        {
            changed(Property.Select(_ => Unit.Default));

            return this;
        }

        public FluentPropertyRegistration<TData> OnChange(Action<IObservable<TData>> changed)
        {
            changed(Property);

            return this;
        }

        public FluentPropertyRegistration<TData> ThenSubscribe(Func<IObservable<TData>, IDisposable> subscribe)
        {
            Actor.AddResource(subscribe(Property));

            return this;
        }

        public static implicit operator UIProperty<TData>(FluentPropertyRegistration<TData> config) => config.Property;
    }
}