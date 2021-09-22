using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tauron.Application.Blazor.UI;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Blazor
{
    public sealed class BindEngine<TModel> : IDisposable
    {
        private readonly object _lock = new();
        private List<BindingRegistration> _bindings = new();
        private IActorComponent? _component;
        private ComponentUIObject? _uiObject;

        internal BindEngine() { }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var binding in _bindings) binding.Binding.Dispose();

                _bindings = null!;
                _component = null;
                _uiObject = null;
            }
        }

        internal void Initialize(ActorComponentBase<TModel> component, IViewModel<TModel> model)
        {
            lock (_lock)
            {
                if (ReferenceEquals(_component, component)) return;

                _component = component;
                _uiObject = new ComponentUIObject(component, null, model);

                UpdateAllBindings();
            }
        }

        public ActiveBinding<TData> Bind<TData>(TData defaultvValue, Expression<Func<TModel, TData>> property)
            => Bind(defaultvValue, Reflex.PropertyName(property));

        public ActiveBinding<TData> Bind<TData>(TData defaultvValue, Expression<Func<TModel, UIProperty<TData>?>> property)
            => Bind(defaultvValue, Reflex.PropertyName(property));

        public ActiveBinding<TData> Bind<TData>(TData defaultvValue, string property)
        {
            lock (_lock)
            {
                var binding = new ActiveBinding<TData>(defaultvValue);
                var registration = new BindingRegistration(property, binding);
                _bindings.Add(registration);
                UpdateSingleBinding(registration);

                return binding;
            }
        }

        private void UpdateAllBindings() => _bindings.ForEach(UpdateSingleBinding);

        private void UpdateSingleBinding(BindingRegistration registration)
        {
            lock (_lock)
            {
                if (_component is null || _uiObject is null)
                    return;

                var (propertyName, internalbinding) = registration;
                internalbinding.Update(
                    new DeferredSource(propertyName, new RootedDataContextPromise(_uiObject)),
                    () => _component.InvokeAsync(_component.StateHasChanged));
            }
        }

        private record BindingRegistration(string PropertyName, IInternalbinding Binding);
    }
}