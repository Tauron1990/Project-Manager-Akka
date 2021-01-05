using System;
using System.Reactive;
using Avalonia.Media;
using JetBrains.Annotations;
using Tauron.Application.Avalonia.AppCore;
using Tauron.Application.Avalonia.UI;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;

namespace Tauron.Application.Avalonia
{
    [PublicAPI]
    public class UserControl : global::Avalonia.Controls.UserControl, IView, IUIElement
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
        
        protected override void OnInitialized()
        {
            Background = Brushes.Transparent;
            base.OnInitialized();
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
    }
}