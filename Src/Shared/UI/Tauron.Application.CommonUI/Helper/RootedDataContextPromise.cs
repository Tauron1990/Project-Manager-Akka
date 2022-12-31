using System;
using System.Reactive.Linq;

namespace Tauron.Application.CommonUI.Helper;

public sealed class RootedDataContextPromise : DataContextPromise
{
    private readonly IUIElement _element;

    private Action? _noContext;

    public RootedDataContextPromise(IUIElement element) => _element = element;

    public override void OnUnload(Action unload)
    {
        if(_element is IView view)
            view.ControlUnload += unload;
    }

    public override void OnContext(Action<IViewModel, IView> modelAction)
    {
        if(_element is IView view)
        {
            if(_element.DataContext is IViewModel model)
            {
                modelAction(model, view);

                return;
            }

            void OnElementOnDataContextChanged(object newValue)
            {
                if(newValue is IViewModel localModel)
                    modelAction(localModel, view);
                else
                    _noContext?.Invoke();
            }

            _element.DataContextChanged.Take(1).Subscribe(OnElementOnDataContextChanged);
        }
        else
        {
            _noContext?.Invoke();
        }
    }

    public override void OnNoContext(Action action)
    {
        _noContext = action;
    }
}