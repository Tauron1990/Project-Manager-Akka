using System;
using NLog;

namespace Tauron.Application.Wpf.SerilogViewer;

public sealed class LoggingEvent : EventArgs
{
    public readonly LogEventInfo EventInfo;

    public LoggingEvent(LogEventInfo logEventInfo) => EventInfo = logEventInfo;


    public static implicit operator LogEventInfo(LoggingEvent e) => e.EventInfo;

    public static implicit operator LoggingEvent(LogEventInfo e) => new(e);
}