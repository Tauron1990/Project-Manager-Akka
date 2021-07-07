using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Client.ViewModels;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class ServiceConfiguration
    {
        public static void Run(IServiceCollection collection)
        {
            collection.AddScoped<IndexViewModel>();
            collection.AddScoped<ConfigurationViewDatabseModel>();

            collection.AddScoped<IClusterConnectionTracker, ClusterConnectionTracker>();
            collection.AddScoped<IDatabaseConfig, DatabaseConfig>();
            collection.AddScoped<IAppIpManager, AppIpManager>();
            collection.AddScoped<IServerInfo, Serverinfo>();

            collection.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}