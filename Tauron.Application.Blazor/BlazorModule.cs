using Autofac;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.Blazor
{
    public class BlazorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DialogCoordinator>().As<IDialogCoordinator>();
            builder.RegisterInstance(BlazorFramework.Dispatcher).As<IUIDispatcher>();
            builder.RegisterType<BlazorFramework>().As<CommonUIFramework>();
        }
    }
}