using Autofac;
using Tauron.Application.CommonUI;
using Tauron.Application.Settings;
using TimeTracker.Data;
using TimeTracker.Managers;
using TimeTracker.ViewModels;
using TimeTracker.Views;

namespace TimeTracker
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AddEntryDialog>().AsSelf();
            builder.RegisterType<CorrectionDialog>().AsSelf();
            builder.RegisterView<MainWindow, MainWindowViewModel>();

            builder.RegisterInstance(SystemClock.Inst).As<SystemClock>();
            builder.RegisterType<DataManager>().AsSelf();
            builder.RegisterType<HolidayManager>().AsSelf();
            builder.RegisterType<ProfileManager>().AsSelf();
            builder.RegisterType<CalculationManager>().AsSelf();
            builder.RegisterType<ConcurancyManager>().AsSelf();

            builder.RegisterSettingsManager(c => c.WithProvider<AppSettingsConfiguration>());

            base.Load(builder);
        }
    }
}