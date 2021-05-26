using System;
using Akka.Util.Extensions;
using Avalonia;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;
using Tauron.Application.Avalonia.AppCore;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Avalonia.UI
{
    [PublicAPI]
    public sealed class ViewModelBinding : UpdatableMarkupExtension
    {
        private readonly string _name;

        public ViewModelBinding(string name) => _name = name;

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget service)
                return "Invalid IProvideValueTarget: " + _name;
            if (service.TargetObject is not AvaloniaObject target)
                return "Invalid Target Object: " + _name;
            if (!ControlBindLogic.FindDataContext(ElementMapper.Create(target).AsOption(), out var promise))
                return "No Data Context Found: " + _name;
            //if (!(ControlBindLogic.FindRoot(target) is IView view))
            //    return "No View as Root: " + _name;

            var connector = new ViewConnector(_name, promise, UpdateValue, AvaloniaFramework.UIDispatcher);

            return connector;
        }
    }
}