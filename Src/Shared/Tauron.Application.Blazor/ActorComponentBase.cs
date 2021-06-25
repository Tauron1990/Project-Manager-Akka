using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Tauron.Akka;
using Tauron.Application.Blazor.UI;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;

namespace Tauron.Application.Blazor
{
    public class ActorComponentBase<TModel> : DispoableComponent, IActorComponent
    {
        private readonly BehaviorSubject<bool> _load = new(false);
        private readonly BehaviorSubject<bool> _unload = new(false);

        private ActorComponentLogic _logic;
        private BindEngine<TModel>? _engine;

        [Parameter]
        public BindEngine<TModel> Engine
        {
            get => _engine ?? throw new ArgumentException(nameof(Engine));
            set => _engine = value;
        }

        public ActorComponentBase()
        {
        }

        void IBinderControllable.Register(string key, IControlBindable bindable, IUIObject affectedPart)
        {
            _logic.Register(key, bindable, affectedPart);
            //lock (_registrations)
            //{
            //    var model = Model;

            //    if (model == null)
            //        throw new InvalidOperationException("Mode is Not set");

            //    var registration = new Registration(bindable.Bind(new ComponentUIObject(this, null, model), affectedPart, model), bindable);
            //    _registrations[key] = registration;
            //    registration.Binding.DisposeWith(this);
            //}
        }

        protected override void OnInitialized()
        {
            _load.OnNext(true);

            Disposable.Create(() =>
            {
                _unload.OnNext(true);
                _unload.OnCompleted();
                _load.OnCompleted();
                _unload.Dispose();
                _load.Dispose();
            })
                      .DisposeWith(this);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            _logic = new ActorComponentLogic(new ComponentUIObject(this, null, Model), Model);
            Engine.Initialize(this, Model);
            base.OnParametersSet();
        }

        void IBinderControllable.CleanUp(string key)
        {
            _logic.CleanUp(key);
            //lock (_registrations)
            //{
            //    if (!_registrations.Remove(key, out var registration)) return;

            //    ((IResourceHolder)this).RemoveResources(registration.Binding);
            //    registration.Binding.Dispose();
            //}
        }

        protected virtual IViewModel<TModel> Model => throw new InvalidOperationException("Model Property was not Overriden");

        IObservable<Unit> IActorComponent.Loaded => _load.Where(b => b).ToUnit();

        IObservable<Unit> IActorComponent.Unloaded => _unload.Where(b => b).ToUnit();

        Task IActorComponent.InvokeAsync(Action action)
        {
            if (IsDisposed) return Task.CompletedTask;
            return InvokeAsync(action);
        }

        void IActorComponent.StateHasChanged()
        {
            try
            {
                if(IsDisposed) return;
                StateHasChanged();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private sealed record Registration(IDisposable Binding, IControlBindable Binder);
    }
}