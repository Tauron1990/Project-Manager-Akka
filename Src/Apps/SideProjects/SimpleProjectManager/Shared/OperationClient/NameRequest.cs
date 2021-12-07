using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Shared.OperationClient;

public sealed record NameResponse(string Name);

public sealed record NameRequest
{
    public async Task<string?> Ask(IActorRef actorRef, ILogger logger)
    {
        try
        {
            var response = await actorRef.Ask<NameResponse>(new NameRequest(), TimeSpan.FromSeconds(10));

            return response.Name;
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException) return null;
            
            logger.LogWarning(e, "Error on ask Name for {Actor}", actorRef.Path);

            return null;
        }
    }
}