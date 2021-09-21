using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignExtensions.Controls;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.UI;
using Tauron.Application.Wpf.AppCore;

namespace TimeTracker.Controls
{
    public abstract class MyMaterialWindow : MaterialWindow, IView, IWindowProvider, IWindow
    {
        private readonly MaterialWindowControlLogic _controlLogic;
        private readonly IWindow _element;

        protected MyMaterialWindow(IViewModel viewModel)
        {
            _element = new WpfWindow(this);
            _controlLogic = new MaterialWindowControlLogic(this, viewModel);
        }

        public void Register(string key, IControlBindable bindable, IUIObject affectedPart) => _controlLogic.Register(key, bindable, affectedPart);

        public void CleanUp(string key) => _controlLogic.CleanUp(key);

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

        private sealed class MaterialWindowControlLogic : ControlLogicBase<MyMaterialWindow>
        {
            internal MaterialWindowControlLogic(MyMaterialWindow userControl, IViewModel model) : base(userControl, model)
            {
                userControl.SizeToContent = SizeToContent.Manual;
                userControl.ShowInTaskbar = true;
                userControl.ResizeMode = ResizeMode.CanResize;
                userControl.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            protected override void WireUpUnloaded()
            {
                UserControl.Closed += (_, _) => UserControlOnUnloaded();
            }
        }
    }
}