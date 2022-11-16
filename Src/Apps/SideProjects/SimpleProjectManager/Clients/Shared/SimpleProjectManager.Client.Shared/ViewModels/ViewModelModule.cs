using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;
using SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;
using SimpleProjectManager.Client.Shared.ViewModels.Pages;
using SimpleProjectManager.Client.Shared.ViewModels.Tasks;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels;

#pragma warning disable GU0011

public sealed class ViewModelModule : IModule
{
    public void Load(IServiceCollection services)
    {
        services.AddTransient<PageNavigation>();
        services.AddTransient<UploadTransaction>();

        services.AddTransient<JobDetailDisplayViewModel>();
        services.AddTransient<JobSidebarViewModel>();
        services.AddTransient<CurrentJobsViewModel>();

        services.AddTransient<EditJobViewModel>();
        services.AddTransient<NewJobViewModel>();
        services.AddTransient<NewJobViewModel>();
        services.AddTransient<DashboardViewModel>();

        services.AddTransient<CriticalErrorsViewModel>();

        services.AddTransient<PendingTaskDisplayViewModel>();

        services.AddTransient<FileManagerViewModel>();
    }
}