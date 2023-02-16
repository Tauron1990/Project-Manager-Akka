using Microsoft.Extensions.DependencyInjection;
using Tauron.AkkaHost;
using Tauron.Application.CommonUI;
using Tauron.Application.Settings;
using TimeTracker.Data;
using TimeTracker.Managers;
using TimeTracker.ViewModels;
using TimeTracker.Views;
using SystemClock = TimeTracker.Managers.SystemClock;

#pragma warning disable GU0011

namespace TimeTracker
{
    public sealed class MainModule : AkkaModule
    {
        public override void Load(IActorApplicationBuilder builder)
        {
            builder.RegisterSettingsManager(c => c.WithProvider<AppSettingsConfiguration>());
        }

        public override void Load(IServiceCollection builder)
        {
            builder.AddTransient<AddEntryDialog>();
            builder.AddTransient<CorrectionDialog>();
            builder.AddTransient<VacationDialog>();

            builder.RegisterView<MainWindow, MainWindowViewModel>();

            builder.AddSingleton(SystemClock.Inst);
            builder.AddSingleton<DataManager>();
            builder.AddSingleton<HolidayManager>();
            builder.AddSingleton<ProfileManager>();
            builder.AddSingleton<CalculationManager>();
            builder.AddSingleton<ConcurancyManager>();
        }
    }
}