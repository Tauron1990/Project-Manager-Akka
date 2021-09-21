#pragma warning disable GU0011

using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Client.Shared.BaseComponents;
using ServiceManager.Client.Shared.Configuration.ConditionEditor;
using ServiceManager.Client.ViewModels;
using ServiceManager.Client.ViewModels.Apps;
using ServiceManager.Client.ViewModels.Configuration;
using ServiceManager.Client.ViewModels.Identity;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared.Api;
using Stl.Fusion.UI;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class ServiceConfiguration
    {
        public static void Run(IServiceCollection collection)
        {
            //collection.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();

            collection.AddTransient<IApiMessageTranslator, ApiMessageTranslator>();
            collection.AddScoped<IErrorMessageProvider, ErrorMessageProvider>();
            collection.AddBlazoredLocalStorage();
            collection.AddScoped<ICookieService, CookieService>();

            collection.AddTransient<AppListViewModel>();
            collection.AddScoped<ConnectToClusterViewModel>();
            collection.AddScoped<ConfigurationViewDatabseModel>();
            collection.AddScoped<ConfigurationOptionsViewModel>();
            collection.AddScoped<ConfigurationViewGlobalConfigModel>();
            collection.AddScoped<AppConfigurationViewModel>();

            collection.AddSingleton<AddHelper>();
            collection.AddSingleton<BasicAppInfoHelper>();
            collection.AddSingleton<DatabaseRequiredComponentHelper>();
            collection.AddSingleton<BasicAppsAlertModel>();

            collection.AddSingleton<IUICommandTracker, UICommandTracker>();
            collection.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}