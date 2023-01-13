using Microsoft.Extensions.Logging;

namespace Tauron.TAkka;

internal static partial class ObservableActorLogger
{
    [LoggerMessage(EventId = 25, Level = LogLevel.Error, Message = "Unhandelt Exception Thrown")]
    public static partial void UnhandledException(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 26, Level = LogLevel.Error, Message = "Error on Process Event")]
    public static partial void EventProcessError(ILogger logger, Exception ex);
}