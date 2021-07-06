using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace ServiceHost.Client.Shared.ConfigurationServer
{
    [PublicAPI]
    public sealed class ConfigurationApi : ISender
    {
        public const string ConfigurationPath = "/ServiceDeamon/ConfigurationManager";

        private readonly IActorRef _api;

        private ConfigurationApi(IActorRef api) => _api = api;

        void ISender.SendCommand(IReporterMessage command) => _api.Tell(command);

        public Task<TResult> Query<TQuery, TResult>(TQuery query, TimeSpan timeout, Action<string>? mesgs = null)
            where TQuery : ResultCommand<ConfigurationApi, TQuery, TResult>, IConfigQuery
            => this.Send(query, timeout, default(TResult), mesgs ?? (_ => { }));

        public Task<TResult> Query<TQuery, TResult>(TimeSpan timeout, Action<string>? mesgs = null)
            where TQuery : ResultCommand<ConfigurationApi, TQuery, TResult>, IConfigQuery, new()
            => Query<TQuery, TResult>(new TQuery(), timeout, mesgs);

        public Task Command<TCommand>(TCommand command, TimeSpan timeout, Action<string>? msgs = null)
            where TCommand : SimpleCommand<ConfigurationApi, TCommand>, IConfigCommand
            => this.Send(command, timeout, msgs ?? (_ => { }));

        public Task<IsAliveResponse> QueryIsAlive(ActorSystem system, TimeSpan timeout)
            => Tauron.Application.AkkaNode.Services.Core.QueryIsAlive.Ask(system, _api, timeout);

        public static ConfigurationApi CreateFromActor(IActorRef actor)
            => new(actor);

        public static ConfigurationApi CreateProxy(ActorSystem system, string name = "ConfigurationManagerProxy")
        {
            var proxy = ClusterSingletonProxy.Props($"/user/{ConfigurationPath}",
                ClusterSingletonProxySettings.Create(system).WithRole("Service-Manager"));
            return new ConfigurationApi(system.ActorOf(proxy, name));
        }
    }
}