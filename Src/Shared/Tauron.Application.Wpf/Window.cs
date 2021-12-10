using System;
using System.Reactive;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.Wpf.AppCore;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf;

[PublicAPI]
public class Window : System.Windows.Window, IView, IWindowProvider, IWindow
{
    private readonly WindowControlLogic _controlLogic;
    private readonly IWindow _element;

    protected Window(IViewModel viewModel)
    {
        _element = (IWindow)ElementMapper.Create(this);
        _controlLogic = new WindowControlLogic(this, viewModel);
    }

    public void Register(string key, IControlBindable bindable, IUIObject affectedPart)
    {
        _controlLogic.Register(key, bindable, affectedPart);
    }

    public void CleanUp(string key)
    {
        _controlLogic.CleanUp(key);
    }

    public event Action? ControlUnload
    {
        add => _controlLogic.ControlUnload += value;
        remove => _controlLogic.ControlUnload -= value;
    }

    IUIObject? IUIObject.GetPerent() => _element.GetPerent();

    public object Object => this;

    IObservable<object> IUIElement.DataContextChanged => _element.DataContextChanged;

    IObservable<Unit> IUIElement.Loaded => _element.Loaded;

    IObservable<Unit> IUIElement.Unloaded => _element.Unloaded;
    Task<bool?> IWindow.ShowDialog() => _element.ShowDialog();

    IWindow IWindowProvider.Window => _element;
}