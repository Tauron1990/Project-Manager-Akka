using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Shared.OperationClient;

public sealed record NameRequest(string Name)
{
    public async Task<string?> Ask(IActorRef actorRef, ILogger logger)
    {
        try
        {
            
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException) return null;
            
            logger.LogWarning(e, "Error on ask Name for {Actor}", actorRef.Path);

            return null;
        }
    }
}