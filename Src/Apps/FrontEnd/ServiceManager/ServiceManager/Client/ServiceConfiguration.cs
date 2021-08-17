using Microsoft.Extensions.DependencyInjection;
using Plk.Blazor.DragDrop;
using ServiceManager.Client.Shared.BaseComponents;
using ServiceManager.Client.ViewModels;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion.UI;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class ServiceConfiguration
    {
        public static void Run(IServiceCollection collection)
        {
            collection.AddBlazorDragDrop();
            
            collection.AddScoped<ConnectToClusterViewModel>();
            collection.AddScoped<ConfigurationViewDatabseModel>();

            collection.AddScoped<ConfigurationOptionsViewModel>();
            collection.AddScoped<ConfigurationViewGlobalConfigModel>();
            collection.AddScoped<AppConfigurationViewModel>();
            
            collection.AddSingleton<BasicAppInfoHelper>();
            collection.AddSingleton<DatabaseRequiredComponentHelper>();
            
            collection.AddSingleton<IUICommandTracker, UICommandTracker>();
            collection.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}