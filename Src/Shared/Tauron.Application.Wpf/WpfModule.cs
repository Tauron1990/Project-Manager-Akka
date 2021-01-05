using Autofac;
using JetBrains.Annotations;
using Tauron.Application.CommonUI;
using Tauron.Application.Wpf.Implementation;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public sealed class WpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CommonUiModule>();
            builder.RegisterType<PackUriHelper>().As<IPackUriHelper>();
            builder.RegisterType<ImageHelper>().As<IImageHelper>().SingleInstance();
            builder.RegisterType<DialogFactory>().As<IDialogFactory>();
            builder.RegisterType<WpfFramework>().As<CommonUIFramework>().SingleInstance();
            builder.Register(_ => WpfFramework.Dispatcher(System.Windows.Application.Current.Dispatcher));

            base.Load(builder);
        }
    }
}