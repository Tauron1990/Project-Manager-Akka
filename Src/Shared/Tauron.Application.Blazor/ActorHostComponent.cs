using System;
using System.Linq.Expressions;
using DynamicData.Binding;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.Blazor
{
    public abstract class ActorHostComponent<TModel> : DispoableComponent
    {
        public BindEngine<TModel> BindEngine { get; }

        protected ActorHostComponent() => BindEngine = new BindEngine<TModel>();

        //public ActiveBinding<TData> Bind<TData>(Expression<Func<TModel, TData>> property)
        //    => BindEngine.Bind(property);

        public ActiveBinding<TData> Bind<TData>(TData defaultvValue, Expression<Func<TModel, UIProperty<TData>>> property)
            => BindEngine.Bind(defaultvValue, property);

        public ActiveBinding<TData> Bind<TData>(TData defaultvValue, string property)
            => BindEngine.Bind(defaultvValue, property);

        public ActiveBinding<IObservableCollection<TData>> Bind<TData>(Expression<Func<TModel, UICollectionProperty<TData>>> property)
            => BindEngine.Bind<IObservableCollection<TData>>(new ObservableCollectionExtended<TData>(), Reflex.PropertyName(property));
    }
}