using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tauron.Application.Blazor.UI;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Blazor
{
    public sealed class BindEngine<TModel>
    {


        private ActorComponent<TModel>? _component;
        private ComponentUIObject? _uiObject;

        internal void Initialize(ActorComponent<TModel> component)
        {
            if(ReferenceEquals(_component, component)) return;

            _component = component;
        }

        public ActiveBinding<TData> Bind<TData>(Expression<Func<TModel, TData>> property)
        {
            string name = Reflex.PropertyName(property);
            var source = new DeferredSource(name, new RootedDataContextPromise(new ActorComponent<>()))
        }
    }
}