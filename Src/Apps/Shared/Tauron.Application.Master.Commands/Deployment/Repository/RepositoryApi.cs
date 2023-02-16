using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository;

[PublicAPI]
public sealed class RepositoryApi : ISender, IQueryIsAliveSupport
{
    public const string RepositoryPath = @"RepositoryManager";

    private readonly IActorRef _repository;

    private RepositoryApi(IActorRef repository) => _repository = repository;
    public static RepositoryApi Empty { get; } = new(ActorRefs.Nobody);

    void ISender.SendCommand(IReporterMessage command) => _repository.Tell(command);

    public Task<IsAliveResponse> QueryIsAlive(ActorSystem system, TimeSpan timeout)
        => AkkaNode.Services.Core.QueryIsAlive.Ask(system, _repository, timeout);

    public static RepositoryApi CreateFromActor(IActorRef manager)
        => new(manager);

    public static RepositoryApi CreateProxy(ActorSystem system, string name = "RepositoryProxy")
    {
        Props? proxy = ClusterSingletonProxy.Props(
            $"/user/{RepositoryPath}",
            ClusterSingletonProxySettings.Create(system).WithRole("UpdateSystem"));

        return new RepositoryApi(system.ActorOf(proxy, name));
    }
}