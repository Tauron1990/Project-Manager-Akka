using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Host;

namespace Tauron.Application.CommonUI
{
    [PublicAPI]
    public sealed class CommonUiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UiAppService>().As<IHostedService>().SingleInstance();
            builder.RegisterType<DialogCoordinator>().As<IDialogCoordinator>().SingleInstance();

            base.Load(builder);
        }
    }
}