using Akka.Event;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public static class TaskExtensions
{
    public static void LogTaskError(this Task task, string errorMessage, ILoggingAdapter logger)
        => task.LogTaskError(exception => logger.Error(exception, errorMessage));
}