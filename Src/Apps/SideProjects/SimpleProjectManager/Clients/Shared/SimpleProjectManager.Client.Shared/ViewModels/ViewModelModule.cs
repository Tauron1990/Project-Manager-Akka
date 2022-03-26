using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;
using SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;
using SimpleProjectManager.Client.Shared.ViewModels.Pages;
using SimpleProjectManager.Client.Shared.ViewModels.Tasks;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels;

public sealed class ViewModelModule : IModule
{
    public void Load(IServiceCollection services)
    {
        services.AddTransient<PageNavigation>();
        services.AddTransient<UploadTransaction>();
        
        services.AddScoped<JobDetailDisplayViewModel>();
        services.AddScoped<JobSidebarViewModel>();
        services.AddScoped<CurrentJobsViewModel>();

        services.AddScoped<EditJobViewModel>();
        services.AddScoped<NewJobViewModel>();
        services.AddScoped<NewJobViewModel>();
        services.AddScoped<DashboardViewModel>();

        services.AddScoped<CriticalErrorsViewModel>();

        services.AddScoped<PendingTaskDisplayViewModel>();

        services.AddScoped<FileManagerViewModel>();
    }
}