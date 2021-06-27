using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Autofac;
using MongoDB.Driver;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.ServiceManager.AppCore;
using Tauron.Application.ServiceManager.AppCore.ServiceDeamon;
using Tauron.Application.ServiceManager.Components.Dialog;
using Tauron.Host;
using Tauron.Operations;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ConfigurationViewModel : UiActor
    {//mongodb://localhost:27017/?readPreference=primary&appname=Service-Manager&ssl=false
        public UIProperty<string> MainDatabseUrl { get; }

        public UIProperty<ICommand>? ResetMainDatabaseUrl { get; }

        public UIProperty<ICommand>? UpdateMainDatabaseUrl { get; }

        public ConfigurationViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, IDatabaseConfig databaseConfig, IActorApplicationLifetime lifetime, IRestartHelper restartHelper, 
            IEventAggregator aggregator) 
            : base(lifetimeScope, dispatcher)
        {
            MainDatabseUrl = RegisterProperty<string>(nameof(MainDatabseUrl))
                            .WithValidator(obs => from str in obs
                                                  select ValidateMongoUrl(str))
                            .WithDefaultValue(databaseConfig.Url);

            ResetMainDatabaseUrl = NewCommad.WithExecute(() => MainDatabseUrl.Set(databaseConfig.Url))
                                            .ThenRegister(nameof(ResetMainDatabaseUrl));

            UpdateMainDatabaseUrl = NewCommad.WithCanExecute(MainDatabseUrl.IsValid)
                                             .WithFlow(obs => (from _ in obs
                                                               from result in this.ShowDialogAsync<ConfirmRestartDialog, bool, Unit>(() => Unit.Default)
                                                               where result
                                                               from data in MainDatabseUrl.Take(1)
                                                               select data)
                                                          .AutoSubscribe(s =>
                                                                         {
                                                                             if (!databaseConfig.SetUrl(s)) return;

                                                                             restartHelper.Restart = true;
                                                                             lifetime.StopApplication();
                                                                         }, aggregator.PublishError))
                                             .ThenRegister(nameof(UpdateMainDatabaseUrl));
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