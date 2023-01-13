using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace Tauron.Application.Master.Commands.KillSwitch;

internal static partial class KillSwitchWatchLog
{
    [LoggerMessage(EventId = 29, Level = LogLevel.Information, Message = "Send ActorUp Back {path}")]
    internal static partial void SendingActorUp(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 30, Level = LogLevel.Information, Message = "Sending Respond {type}")]
    internal static partial void SendingRespond(ILogger logger, KillRecpientType type);

    [LoggerMessage(EventId = 31, Level = LogLevel.Information, Message = "Leaving Cluster")]
    internal static partial void LeavingCluster(ILogger logger);
}