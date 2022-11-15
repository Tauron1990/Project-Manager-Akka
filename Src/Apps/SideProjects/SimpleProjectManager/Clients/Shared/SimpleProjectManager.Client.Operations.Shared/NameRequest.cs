using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Client.Operations.Shared;

public sealed record NameRequest
{
    public static async Task<ObjectName?> Ask(IActorRef actorRef, ILogger logger)
    {
        try
        {
            var response = await actorRef.Ask<NameResponse>(new NameRequest(), TimeSpan.FromSeconds(20)).ConfigureAwait(false);

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