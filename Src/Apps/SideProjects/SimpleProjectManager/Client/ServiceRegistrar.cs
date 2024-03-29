﻿using MudBlazor;
using MudBlazor.Services;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Client.Shared.ViewModels.Devices;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Client.ViewModels.Devices;
using SimpleProjectManager.Client.ViewModels.LogFiles;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace SimpleProjectManager.Client;

#pragma warning disable GU0011

public static class ServiceRegistrar
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.UseMicrosoftDependencyResolver(); //Splat config
        IMutableDependencyResolver resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI();

        ViewModelModule.LoadStatic(services);

        services.AddMudServices(c => c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter);

        services.AddTransient<FileDetailDisplayViewModel>();
        services.AddTransient<JobPriorityViewModel>();

        services.AddTransient<JobEditorViewModel>();
        services.AddTransient<JobEditorViewModelBase>(s => s.GetRequiredService<JobEditorViewModel>());

        services.AddTransient<FileUploaderViewModel>();
        services.AddTransient<FileUploaderViewModelBase>(s => s.GetRequiredService<FileUploaderViewModel>());

        services.AddTransient<CriticalErrorViewModel>();

        services.AddTransient<DeviceDisplayViewModel>();
        services.AddTransient<DevicesViewModel>();
        services.AddTransient<DeviceInputViewModel>();
        services.AddTransient<SingleSensorViewModel>();
        services.AddTransient<SingleButtonViewModel>();
        services.AddTransient<LogsDisplayViewModel>();

        services.AddTransient<LogFilesViewModel>();
        services.AddTransient<LogFileDisplayViewModel>();
    }
}