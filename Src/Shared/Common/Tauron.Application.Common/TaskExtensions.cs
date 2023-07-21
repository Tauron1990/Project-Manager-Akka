using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Tauron;

[PublicAPI]
public static class TaskExtensions
{
    public static void LogTaskError(this Task task, string errorMessage, ILogger logger)
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        => LogTaskError(task, exception => logger.LogError(exception, errorMessage));

    public static async void LogTaskError(this Task task, Action<Exception> onError)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            onError(e);
        }
    }

    public static void Ignore(this Task _) { }

    public static void Ignore<T>(this Task<T> _) { }

    public static void Ignore(this ValueTask _) { }

    public static void Ignore<T>(this ValueTask<T> _) { }
}