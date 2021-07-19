﻿using Microsoft.Extensions.DependencyInjection;
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
            collection.AddScoped<ConnectToClusterViewModel>();
            collection.AddScoped<IndexViewModel>();
            collection.AddScoped<ConfigurationViewDatabseModel>();

            collection.AddTransient<ConfigurationOptionsViewModel>();
            collection.AddTransient<ConfigurationViewGlobalConfigModel>();
            collection.AddTransient<AppConfigurationViewModel>();

            collection.AddScoped<IClusterConnectionTracker, ClusterConnectionTracker>();
            collection.AddScoped<IDatabaseConfig, DatabaseConfig>();
            collection.AddScoped<IAppIpManager, AppIpManager>();
            collection.AddScoped<IServerInfo, Serverinfo>();
            collection.AddScoped<IServerConfigurationApi, ConfigurationApiModel>();

            collection.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}