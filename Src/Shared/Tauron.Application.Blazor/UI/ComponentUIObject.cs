using System;
using System.Reactive;
using System.Reactive.Linq;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;

namespace Tauron.Application.Blazor.UI
{
    public sealed class ComponentUIObject : IView, IUIElement
    {
        private readonly IActorComponent _target;
        private readonly IUIObject? _parent;

        public ComponentUIObject(IActorComponent target, IUIObject? parent, IViewModel model)
        {
            _target = target;
            _parent = parent;
            DataContext = model;

            target.Unloaded.Subscribe(_ => ControlUnload?.Invoke());
        }

        public object Object => _target;

        public IUIObject? GetPerent() => _parent;

        public object? DataContext { get; set; }

        public IObservable<object> DataContextChanged { get; } = Observable.Empty<object>();

        public IObservable<Unit> Loaded => _target.Loaded;

        public IObservable<Unit> Unloaded => _target.Unloaded;

        public void Register(string key, IControlBindable bindable, IUIObject affectedPart) => _target.Register(key, bindable, affectedPart);

        public void CleanUp(string key) => _target.CleanUp(key);

        public event Action? ControlUnload;
    }
}