using System;
using Akka.Actor;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed record RegisterService(Type Interface, IActorRef Host);

    public sealed record RegisterServiceResponse(Exception? Error);
    
    public sealed record UnregisterService(IActorRef Host);

    public sealed record ResolveService(Type Interface);

    public sealed record ResolveResponse(IActorRef Host, Exception? Error);
}