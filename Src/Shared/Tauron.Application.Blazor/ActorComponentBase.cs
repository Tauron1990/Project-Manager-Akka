using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Tauron.Application.Blazor.UI;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;

namespace Tauron.Application.Blazor
{
    public class ActorComponentBase<TModel> : DispoableComponent, IActorComponent
    {
        private readonly BehaviorSubject<bool> _load = new(false);
        private readonly BehaviorSubject<bool> _unload = new(false);
        private BindEngine<TModel>? _engine;

        private ActorComponentLogic? _logic;

        protected ActorComponentBase() { }

        [Parameter]
        public BindEngine<TModel> Engine
        {
            get => _engine ?? throw new ArgumentException(nameof(Engine));
            set => _engine = value;
        }

        protected virtual IViewModel<TModel> Model => throw new InvalidOperationException("Model Property was not Overriden");

        void IBinderControllable.Register(string key, IControlBindable bindable, IUIObject affectedPart)
        {
            if (_logic == null)
                throw new InvalidOperationException("ActorComponentLogic not Initialized");

            _logic.Register(key, bindable, affectedPart);
        }

        void IBinderControllable.CleanUp(string key)
        {
            if (_logic == null)
                throw new InvalidOperationException("ActorComponentLogic not Initialized");

            _logic.CleanUp(key);
        }

        IObservable<Unit> IActorComponent.Loaded => _load.Where(b => b).ToUnit();

        IObservable<Unit> IActorComponent.Unloaded => _unload.Where(b => b).ToUnit();

        Task IActorComponent.InvokeAsync(Action action)
        {
            return IsDisposed ? Task.CompletedTask : InvokeAsync(action);
        }

        void IActorComponent.StateHasChanged()
        {
            try
            {
                if (IsDisposed) return;

                StateHasChanged();
            }
            catch (ObjectDisposedException) { }
        }

        protected override void OnInitialized()
        {
            _load.OnNext(true);

            Disposable.Create(
                    (_unload, _load),
                    s =>
                    {
                        var (unload, load) = s;
                        unload.OnNext(true);
                        unload.OnCompleted();
                        load.OnCompleted();
                        unload.Dispose();
                        load.Dispose();
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

        //private sealed record Registration(IDisposable Binding, IControlBindable Binder);
    }
}