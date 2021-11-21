using System;
using System.Threading.Tasks;
using Akka.Event;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Tauron;

[PublicAPI]
public static class TaskExtensions
{
    public static void LogTaskError(this Task task, string errorMessage, ILoggingAdapter logger)
        => LogTaskError(task, exception => logger.Error(exception, errorMessage));

    public static void LogTaskError(this Task task, string errorMessage, ILogger logger)
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        => LogTaskError(task, exception => logger.LogError(exception, errorMessage));

    public static void LogTaskError(this Task task, Action<Exception> onError)
        #pragma warning disable AV2235
        => task.ContinueWith(
            compled =>
            {
                if (compled.IsCompletedSuccessfully) return;

                if (compled.IsCanceled)
                {
                    onError(new TaskCanceledException(compled));
                }
                else if (compled.IsFaulted)
                {
                    var err = compled.Exception.Unwrap();
                    if (err != null)
                        onError(err);
                }
            });
    #pragma warning restore AV2235

    public static void Ignore(this Task _) { }

    public static void Ignore<T>(this Task<T> _) { }
    
    public static void Ignore(this ValueTask _) { }

    public static void Ignore<T>(this ValueTask<T> _) { }
}