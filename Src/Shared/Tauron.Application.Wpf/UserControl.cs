using System;
using System.Reactive;
using System.Windows.Media;
using JetBrains.Annotations;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.Wpf.AppCore;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public class UserControl : System.Windows.Controls.UserControl, IView, IUIElement
    {
        private readonly UserControlLogic _controlLogic;
        private readonly IUIElement _element;

        protected UserControl(IViewModel viewModel)
        {
            _element = (IUIElement)ElementMapper.Create(this);
            _controlLogic = new UserControlLogic(this, viewModel);
        }

        public void Register(string key, IControlBindable bindable, IUIObject affectedPart)
        {
            _controlLogic.Register(key, bindable, affectedPart);
        }

        protected override void OnInitialized(EventArgs e)
        {
            Background = Brushes.Transparent;
            base.OnInitialized(e);
        }

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
    }
}