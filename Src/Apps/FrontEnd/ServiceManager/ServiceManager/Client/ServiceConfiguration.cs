using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Client.Shared.BaseComponents;
using ServiceManager.Client.Shared.Configuration.ConditionEditor;
using ServiceManager.Client.ViewModels;
using ServiceManager.Client.ViewModels.Identity;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared.Identity;
using Stl.Fusion.UI;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class ServiceConfiguration
    {
        public static void Run(IServiceCollection collection)
        {
            //collection.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();

            collection.AddScoped<IErrorMessageProvider, ErrorMessageProvider>();
            collection.AddBlazoredLocalStorage();

            collection.AddScoped<IAccountService, ClientAccountService>();

            collection.AddScoped<ConnectToClusterViewModel>();
            collection.AddScoped<ConfigurationViewDatabseModel>();
            collection.AddScoped<ConfigurationOptionsViewModel>();
            collection.AddScoped<ConfigurationViewGlobalConfigModel>();
            collection.AddScoped<AppConfigurationViewModel>();

            collection.AddSingleton<AddHelper>();
            collection.AddSingleton<BasicAppInfoHelper>();
            collection.AddSingleton<DatabaseRequiredComponentHelper>();
            
            collection.AddSingleton<IUICommandTracker, UICommandTracker>();
            collection.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}