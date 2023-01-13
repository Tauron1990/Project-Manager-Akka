using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.Core;

[PublicAPI]
public sealed class EventSubscribtion : IDisposable
{
    private readonly Type _event;
    private readonly IActorRef _eventSource;

    public EventSubscribtion(Type @event, IActorRef eventSource)
    {
        _event = @event;
        _eventSource = eventSource;
    }

    public static EventSubscribtion Empty { get; } = new(typeof(Type), ActorRefs.Nobody);

    public void Dispose()
    {
        if(_eventSource.IsNobody()) return;

        _eventSource.Tell(new EventUnSubscribe(_event));
    }
}