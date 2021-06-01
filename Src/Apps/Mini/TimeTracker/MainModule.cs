using Autofac;
using Tauron.Application.CommonUI;
using Tauron.Application.Settings;
using TimeTracker.Data;
using TimeTracker.ViewModels;
using TimeTracker.Views;

namespace TimeTracker
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CorrectionDialog>().AsSelf();
            builder.RegisterView<MainWindow, MainWindowViewModel>();

            builder.RegisterSettingsManager(c => c.WithProvider<AppSettingsConfiguration>());

            base.Load(builder);
        }
    }
}