using System;
using System.Diagnostics;
using NLog;
using NLog.Targets;

namespace Tauron.Application.Wpf.SerilogViewer
{
    public sealed class LoggerViewerSink : Target
    {
        public LoggerViewerSink()
        {
            Name = "LoggerViewSink";
            CurrentSink = this;
        }

        internal static LoggerViewerSink? CurrentSink { get; private set; }

        public LimitedList<LogEventInfo> Logs { get; } = new(150);

        public event Action<LogEventInfo>? LogReceived;

        [DebuggerHidden]
        protected override void Write(LogEventInfo logEvent)
        {
            Logs.Add(logEvent);
            LogReceived?.Invoke(logEvent);
            base.Write(logEvent);
        }
    }
}