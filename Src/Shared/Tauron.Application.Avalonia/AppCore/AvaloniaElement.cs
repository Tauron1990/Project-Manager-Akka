using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Avalonia.AppCore
{
    public class AvaloniaElement : AvaObject, IUIElement
    {
        private readonly StyledElement _element;

        public object DataContext
        {
            get => _element.DataContext;
            set => _element.DataContext = value;
        }

        public AvaloniaElement(StyledElement element)
            : base(element) => _element = element;

        public IObservable<object> DataContextChanged => _element.GetPropertyChangedObservable(StyledElement.DataContextProperty);

        public virtual IObservable<Unit> Loaded
            => _element
              .GetObservable(StyledElement.ParentProperty)
              .NotDefault()
              .ToUnit();

        public virtual IObservable<Unit> Unloaded
            => _element
              .GetObservable(StyledElement.ParentProperty)
              .Where(se => se == null)
              .ToUnit();
    }
}