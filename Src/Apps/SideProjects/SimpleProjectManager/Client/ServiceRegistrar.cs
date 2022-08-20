using MudBlazor;
using MudBlazor.Services;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Client.ViewModels;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Tauron;
using Tauron.Applicarion.Redux.Configuration;

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
        
        services.AddTransient<FileDetailDisplayViewModel>();
        services.AddTransient<JobPriorityViewModel>();
        
        services.AddTransient<JobEditorViewModel>();
        services.AddTransient<FileUploaderViewModel>();
        
        services.AddTransient<CriticalErrorViewModel>();
    }
}