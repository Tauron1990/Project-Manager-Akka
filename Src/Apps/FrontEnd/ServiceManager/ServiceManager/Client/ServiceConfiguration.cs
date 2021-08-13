﻿using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Client.ViewModels;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class ServiceConfiguration
    {
        public static void Run(IServiceCollection collection)
        {
            collection.AddScoped<ConnectToClusterViewModel>();
            collection.AddScoped<ConfigurationViewDatabseModel>();

            collection.AddScoped<ConfigurationOptionsViewModel>();
            collection.AddScoped<ConfigurationViewGlobalConfigModel>();
            collection.AddScoped<AppConfigurationViewModel>();
            collection.AddScoped<ConfigurationViewAppConfigModel>();

            collection.AddScoped<IDatabaseConfigOld, DatabaseConfig>();
            collection.AddScoped<IServerConfigurationApi, ConfigurationApiModel>();

            collection.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}