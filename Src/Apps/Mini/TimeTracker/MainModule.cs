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
            builder.RegisterType<VacationDialog>().AsSelf();

            builder.RegisterView<MainWindow, MainWindowViewModel>();

            builder.RegisterInstance(SystemClock.Inst).As<SystemClock>().SingleInstance();
            builder.RegisterType<DataManager>().AsSelf().SingleInstance();
            builder.RegisterType<HolidayManager>().AsSelf().SingleInstance();
            builder.RegisterType<ProfileManager>().AsSelf().SingleInstance();
            builder.RegisterType<CalculationManager>().AsSelf().SingleInstance();
            builder.RegisterType<ConcurancyManager>().AsSelf().SingleInstance();

            builder.RegisterSettingsManager(c => c.WithProvider<AppSettingsConfiguration>());

            base.Load(builder);
        }
    }
}