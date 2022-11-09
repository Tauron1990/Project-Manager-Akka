using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Client.Operations.Shared;

public sealed record NameResponse(ObjectName? Name);

internal static partial class NameRequestLog
{
    [LoggerMessage(EventId = 48, Level = LogLevel.Error, Message = "Error on ask for Name to Actor {path}")]
    internal static partial void NameRequestTimeout(ILogger logger, Exception ex, ActorPath path);
}

public sealed record NameRequest
{
    public static async Task<ObjectName?> Ask(IActorRef actorRef, ILogger logger)
    {
        try
        {
            var response = await actorRef.Ask<NameResponse>(new NameRequest(), TimeSpan.FromSeconds(20));

            return response.Name;
        }
        catch (Exception e)
        {
            if(e is OperationCanceledException) return null;

            NameRequestLog.NameRequestTimeout(logger, e, actorRef.Path);

            return null;
        }
    }
}