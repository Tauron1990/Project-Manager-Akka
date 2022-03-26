using MudBlazor;
using MudBlazor.Services;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;
using SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;
using SimpleProjectManager.Client.Shared.ViewModels.Pages;
using SimpleProjectManager.Client.Shared.ViewModels.Tasks;
using SimpleProjectManager.Client.ViewModels;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Tauron;

namespace SimpleProjectManager.Client;

public static class ServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.UseMicrosoftDependencyResolver(); //Splat config
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI();

        services.RegisterModule<ViewModelModule>();
        
        services.AddMudServices(c => c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter);
        
        services.AddScoped<FileDetailDisplayViewModel>();
        services.AddScoped<JobPriorityViewModel>();
        
        services.AddScoped<JobEditorViewModel>();
        services.AddScoped<FileUploaderViewModel>();
        
        services.AddScoped<CriticalErrorViewModel>();
    }
}