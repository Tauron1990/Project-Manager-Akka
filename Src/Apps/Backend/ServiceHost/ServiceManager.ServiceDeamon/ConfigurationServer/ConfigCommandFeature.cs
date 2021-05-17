using System;
using System.Linq;
using System.Reactive;
using Akka.Actor;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Akka.Util;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.ServiceDeamon.ConfigurationServer.Data;
using ServiceManager.ServiceDeamon.ConfigurationServer.Internal;
using SharpRepository.Repository;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Operations;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public class ConfigCommandFeature : ReportingActor<ConfigFeatureConfiguration>
    {
        private static readonly Guid Seednamespace = new("08C98B41-9FF3-4A40-BAF6-3A4FD3810D89");

        protected override void ConfigImpl()
        {
            (from evt in CurrentState.EventPublisher
             where evt is ServerConfigurationEvent
             select ((ServerConfigurationEvent) evt).Configugration)
               .AutoSubscribe(s => Self.Tell(s)).DisposeWith(this);

            TryReceive<UpdateServerConfigurationCommand>(nameof(UpdateServerConfigurationCommand),
                obs => obs
                      .Select(m => m.Event.ServerConfigugration)
                      .UForwardToParent()
                      .Select(_ => OperationResult.Success()));

            Option<TData> UpdateRepo<TData>(IRepository<TData, string> repo, ConfigDataAction action, Option<TData> dataOption) 
                where TData : class
            {
                dataOption.OnSuccess(data =>
                                     {
                                         switch (action)
                                         {
                                             case ConfigDataAction.Delete:
                                                 repo.Delete(data);
                                                 break;
                                             case ConfigDataAction.Update:
                                                 repo.Update(data);
                                                 break;
                                             case ConfigDataAction.Create:
                                                 repo.Add(data);
                                                 break;
                                             default:
                                                 throw new ArgumentOutOfRangeException(nameof(action), action, null);
                                         }
                                     });

                return dataOption;
            }

            static Option<Unit> SendEvent(ISubject<IConfigEvent> eventSink, Option<IConfigEvent> evtOption)
                => evtOption.Select(evt =>
                                    {
                                        eventSink.OnNext(evt);
                                        return Unit.Default;
                                    });

            TryReceive<UpdateSeedUrlCommand>(nameof(UpdateSeedUrlCommand),
                obs => from request in obs
                       from dataOption in Task.Run(() => UpdateRepo(
                                              request.State.Seeds,
                                              request.Event.Action, 
                                              new SeedUrlEntity(
                                                  Deterministic.Create(Seednamespace, request.Event.SeedUrl.Url).ToString("N"),
                                                  request.Event.SeedUrl))) 
                       let opt = SendEvent(request.State.EventPublisher, dataOption.Select<IConfigEvent>(data => new SeedDataEvent(data.Url, request.Event.Action)))
                       select opt.Select(_ => OperationResult.Success()));

            TryReceive<UpdateGlobalConfigCommand>(nameof(UpdateGlobalConfigCommand),
                obs => from request in obs
                       from data in Task.Run(() => UpdateRepo(request.State.GlobalRepository, request.Event.Action, new GlobalConfigEntity(GlobalConfigEntity.EntityId, request.Event.Config))) 
                       let opt = SendEvent(request.State.EventPublisher, data.Select<IConfigEvent>(d => new GlobalConfigEvent(d.Config, request.Event.Action)))
                       select opt.Select(_ => OperationResult.Success()));

            TryReceive<UpdateSpecificConfigCommand>(nameof(UpdateSpecificConfigCommand),
                obs => from request in obs
                       let evt = request.Event
                       let state = request.State
                       from data in Task.Run(() => evt.Action == ConfigDataAction.Update
                                                 ? state.Apps.Get(request.Event.Id).OptionNotNull()
                                                 : SpecificConfigEntity.From(evt))
                       let newData = UpdateRepo(state.Apps, evt.Action, data)
                       let opt = SendEvent(state.EventPublisher, newData.Select<IConfigEvent>(d => new SpecificConfigEvent(d.Config, evt.Action)))
                       select opt.Select(_ => OperationResult.Success(), () => OperationResult.Failure(ConfigError.SpecificConfigurationNotFound)));

            TryReceive<UpdateConditionCommand>(nameof(UpdateConditionCommand),
                obs => from request in obs
                       let evt = request.Event
                       let state = request.State
                       from data in Task.Run(() => state.Apps.Get(evt.Name).OptionNotNull())
                       let newData = data.Select(e => e with
                                                      {
                                                          Config = e.Config with
                                                                   {
                                                                       Conditions = evt.Action switch
                                                                       {
                                                                           ConfigDataAction.Delete => e.Config.Conditions.Remove(evt.Condition),
                                                                           ConfigDataAction.Update => e.Config.Conditions.Replace(
                                                                               e.Config.Conditions.First(c => c.Name == evt.Condition.Name),
                                                                               evt.Condition),
                                                                           ConfigDataAction.Create => e.Config.Conditions.Add(evt.Condition),
                                                                           _ => e.Config.Conditions
                                                                       }
                                                                   }
                                                      })
                       let updateData = UpdateRepo(state.Apps, ConfigDataAction.Update, newData)
                       let result = SendEvent(state.EventPublisher, updateData.Select<IConfigEvent>(e => new ConditionUpdateEvent(evt.Condition, evt.Name, evt.Action)))
                       select result.Select(_ => OperationResult.Success(), () => OperationResult.Failure(ConfigError.SpecificConfigurationNotFound)));

            TryReceive<ForceUpdateHostConfigCommand>(nameof(ForceUpdateHostConfigCommand),
                obs => obs.Do(m => m.State.EventPublisher.OnNext(new ForceHostUpdate()))
                          .Select(_ => OperationResult.Success()));
        }
    }
}