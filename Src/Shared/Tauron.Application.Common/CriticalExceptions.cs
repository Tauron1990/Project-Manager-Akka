using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public static class CriticalExceptions
{
    public static bool IsCriticalApplicationException(this Exception? ex)
    {
        ex = Unwrap(ex);

        return ex is StackOverflowException or OutOfMemoryException or ThreadAbortException or SecurityException;
    }


    public static bool IsCriticalException(this Exception? ex)
    {
        ex = Unwrap(ex);

        return ex is NullReferenceException or StackOverflowException or OutOfMemoryException or ThreadAbortException or SEHException or SecurityException;
    }


    public static Exception? Unwrap(this Exception? ex)
    {
        if (ex is AggregateException ex2) return ex2.Flatten().InnerExceptions[0];

        while (ex?.InnerException != null && ex is TargetInvocationException or TypeLoadException) ex = ex.InnerException;

        return ex;
    }
}