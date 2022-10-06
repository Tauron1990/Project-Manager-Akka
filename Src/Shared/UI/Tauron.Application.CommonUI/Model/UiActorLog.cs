using System;
using Microsoft.Extensions.Logging;

namespace Tauron.Application.CommonUI.Model;

internal static partial class UiActorLog
{
    [LoggerMessage(EventId = 55, Level = LogLevel.Information, Message = "Execute Command  {command}")]
    public static partial void ExecuteCommand(ILogger log, string command);

    [LoggerMessage(EventId = 56, Level = LogLevel.Warning, Message = "Command not Found {command}")]
    public static partial void CommandNotFound(ILogger logger, string command);

    [LoggerMessage(EventId = 57, Level = LogLevel.Information, Message = "Ui Actor Terminated {type}")]
    public static partial void UiActorTerminated(ILogger logger, Type type);
    
    [LoggerMessage(EventId = 58, Level = LogLevel.Information, Message = "Execute Event  {evntName}")]
    public static partial void ExecuteEvent(ILogger log, string evntName);

    [LoggerMessage(EventId = 59, Level = LogLevel.Warning, Message = "Event not Found {evntName}")]
    public static partial void EventNotFound(ILogger logger, string evntName);
    
    [LoggerMessage(EventId = 60, Level = LogLevel.Information, Message = "Track property {name}")]

    public static partial void TrackProperty(ILogger logger, string name);
}