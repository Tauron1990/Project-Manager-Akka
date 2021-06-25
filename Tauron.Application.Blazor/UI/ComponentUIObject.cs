using System;
using System.Reactive;
using System.Reactive.Linq;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Blazor.UI
{
    public sealed class ComponentUIObject : IUIElement
    {
        private readonly IActorComponent _target;
        private readonly IUIObject? _parent;

        public ComponentUIObject(IActorComponent target, IUIObject? parent, IViewModel model)
        {
            _target = target;
            _parent = parent;
            DataContext = model;
        }

        public object Object => _target;

        public IUIObject? GetPerent() => _parent;

        public object? DataContext { get; set; }

        public IObservable<object> DataContextChanged { get; } = Observable.Empty<object>();

        public IObservable<Unit> Loaded => _target.Loaded;

        public IObservable<Unit> Unloaded => _target.Unloaded;
    }
}