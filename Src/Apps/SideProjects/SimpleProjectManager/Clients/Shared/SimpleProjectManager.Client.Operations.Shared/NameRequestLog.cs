using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Client.Operations.Shared;

internal static partial class NameRequestLog
{
    [LoggerMessage(EventId = 48, Level = LogLevel.Error, Message = "Error on ask for Name to Actor {path}")]
    internal static partial void NameRequestTimeout(ILogger logger, Exception ex, ActorPath path);
}