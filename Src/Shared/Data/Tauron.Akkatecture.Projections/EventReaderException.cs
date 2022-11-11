using System;
using JetBrains.Annotations;

namespace Tauron.Akkatecture.Projections;

[PublicAPI]
public sealed class EventReaderException : EventArgs
{
    public EventReaderException(string tag, Exception exception)
    {
        Tag = tag;
        Exception = exception;
    }

    public string Tag { get; }

    public Exception Exception { get; }
}