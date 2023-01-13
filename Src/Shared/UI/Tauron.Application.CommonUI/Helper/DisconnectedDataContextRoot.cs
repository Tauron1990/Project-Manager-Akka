using System;
using System.Reactive.Linq;

namespace Tauron.Application.CommonUI.Helper;

public sealed class DisconnectedDataContextRoot : DataContextPromise
{
    private Action<IViewModel, IView>? _modelAction;
    private Action? _noContext;
    private Action? _unload;

    public DisconnectedDataContextRoot(IUIElement elementBase)
    {
        void OnLoad()
        {
            (bool hasValue, IBinderControllable? value) = ControlBindLogic.FindRoot(elementBase.AsOption<IUIObject>());
            if(hasValue && value is IView control and IUIElement { DataContext: IViewModel model })
            {
                _modelAction?.Invoke(model, control);

                if(_unload != null)
                    control.ControlUnload += _unload;
            }
            else
                _noContext?.Invoke();
        }

        elementBase.Loaded.Take(1).Subscribe(_ => OnLoad());
    }

    public override void OnUnload(Action unload) => _unload = unload;

    public override void OnContext(Action<IViewModel, IView> modelAction) => _modelAction = modelAction;

    public override void OnNoContext(Action noContext) => _noContext = noContext;
}