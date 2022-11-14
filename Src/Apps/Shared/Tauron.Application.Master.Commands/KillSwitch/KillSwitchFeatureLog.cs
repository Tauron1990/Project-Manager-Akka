using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace Tauron.Application.Master.Commands.KillSwitch;

internal static partial class KillSwitchFeatureLog
{
    [LoggerMessage(EventId = 32, Level = LogLevel.Information, Message = "Remove KillSwitch Actor {path}")]
    internal static partial void RemoveKillswitch(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 33, Level = LogLevel.Information, Message = "NewKillSwitch Actor {path}")]
    internal static partial void NewKillSwitch(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 34, Level = LogLevel.Information, Message = "Set KillSwitch Actor Type {type} {path}")]
    internal static partial void KillSwitchActorType(ILogger logger, KillRecpientType type, ActorPath path);

    [LoggerMessage(EventId = 35, Level = LogLevel.Information, Message = "Begin Cluster Shutdown")]
    internal static partial void BeginClusterShutdown(ILogger logger);

    [LoggerMessage(EventId = 36, Level = LogLevel.Information, Message = "Incomming KillSwitch Actor {path}")]
    internal static partial void IncommingKillWatcher(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 37, Level = LogLevel.Information, Message = "Send Kill Message for {type} to {count} Actors")]
    internal static partial void TellClusterShutdown(ILogger logger, KillRecpientType type, int count);
}