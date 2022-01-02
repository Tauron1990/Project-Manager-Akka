using MudBlazor;
using MudBlazor.Services;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Tauron.Application;

namespace SimpleProjectManager.Client;

public class ServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.UseMicrosoftDependencyResolver(); //Splat config
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI();

        services.AddMudServices(c => c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter);
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddTransient<PageNavigation>();
        services.AddTransient<UploadTransaction>();
        
        services.AddScoped<FileDetailDisplayViewModel>();
        services.AddScoped<JobDetailDisplayViewModel>();
        services.AddScoped<JobPriorityViewModel>();
        services.AddScoped<JobSidebarViewModel>();
        services.AddScoped<CurrentJobsViewModel>();

        services.AddScoped<EditJobViewModel>();
        services.AddScoped<JobEditorViewModel>();
        services.AddScoped<FileUploaderViewModel>();
        services.AddScoped<NewJobViewModel>();
        services.AddScoped<NewJobViewModel>();
        services.AddScoped<DashboardViewModel>();

        services.AddScoped<CriticalErrorsViewModel>();
        services.AddScoped<CriticalErrorViewModel>();

        services.AddScoped<PendingTaskDisplayViewModel>();

        services.AddScoped<FileManagerViewModel>();
    }
}