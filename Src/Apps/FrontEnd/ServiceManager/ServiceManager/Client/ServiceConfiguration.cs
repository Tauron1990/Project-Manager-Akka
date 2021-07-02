using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Client.ViewModels;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class ServiceConfiguration
    {
        public static void Run(IServiceCollection collection)
        {
            collection.AddScoped<IEventAggregator, EventAggregator>();
            collection.AddScoped<ConfigurationViewDatabseModel>();
        }
    }
}