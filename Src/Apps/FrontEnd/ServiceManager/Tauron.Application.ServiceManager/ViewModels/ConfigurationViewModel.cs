using System.Reactive.Linq;
using Autofac;
using MongoDB.Driver;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.ServiceManager.AppCore.ServiceDeamon;
using Tauron.Operations;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ConfigurationViewModel : UiActor
    {
        public UIProperty<string> MainDatabseUrl { get; }

        public ConfigurationViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, IDatabaseConfig databaseConfig) 
            : base(lifetimeScope, dispatcher)
        {
            MainDatabseUrl = RegisterProperty<string>(nameof(MainDatabseUrl))
                            .WithValidator(obs => from str in obs
                                                  select ValidateMongoUrl(str))
                            .WithDefaultValue(databaseConfig.Url);
        }


        private static Error? ValidateMongoUrl(string url)
        {
            try
            { 
                MongoUrl.Create(url);
                return null;
            }
            catch (MongoConfigurationException e)
            {
                return new Error(e.Message, nameof(MongoConfigurationException));
            }
        }
    }
}