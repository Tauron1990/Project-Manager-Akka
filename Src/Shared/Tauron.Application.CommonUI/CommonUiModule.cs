using Autofac;
using JetBrains.Annotations;
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
            builder.RegisterType<AppLifetime>().Named<IAppRoute>("default");
            builder.RegisterType<DialogCoordinator>().As<IDialogCoordinator>().SingleInstance();

            base.Load(builder);
        }
    }
}