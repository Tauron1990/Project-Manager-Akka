using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Features;

namespace Tauron.Application.Master.Commands.Administration.Host;

[PublicAPI]
public sealed class HostApi
{
    public const string ApiKey = "HostApi";

    private static readonly object Lock = new();

    private static HostApi? _hostApi;

    private readonly IActorRef _actorRef;

    public HostApi(IActorRef actorRef)
        => _actorRef = actorRef;

    public static HostApi CreateOrGet(ActorSystem actorRefFactory)
    {
        lock (Lock)
        {
            return _hostApi ??= new HostApi(actorRefFactory.ActorOf(HostApiManagerFeature.Create(), SubscribeFeature.New()));
        }
    }

    public Task<TResult> ExecuteCommand<TResult>(InternalHostMessages.CommandBase<TResult> command)
        where TResult : OperationResponse, new()
        => _actorRef.Ask<TResult>(command, TimeSpan.FromMinutes(1));

    public Task<ImmutableList<HostApp>> QueryApps(string name)
        => _actorRef
           .Ask<HostAppsResponse>(new QueryHostApps(name), TimeSpan.FromSeconds(30))
           .ContinueWith(t => t.Result.Apps);

    public EventSubscribtion Event<T>() => _actorRef.SubscribeToEvent<T>();
}