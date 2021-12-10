using System;
using System.Globalization;
using System.Windows.Media;
using NLog;
using NLog.Targets;

namespace Tauron.Application.Wpf.SerilogViewer;

public class LogEventViewModel
{
    private static readonly RenderHelper Helper = new();

    public LogEventViewModel(LoggingEvent info)
    {
        Info = info;
        var logEventInfo = info.EventInfo;

        var msg = Helper.Render(logEventInfo);

        ToolTip = msg;
        Level = logEventInfo.Level.ToString();
        FormattedMessage = msg;
        Exception = logEventInfo.Exception;
        LoggerName = Helper.GetLoggerName(logEventInfo);
        Time = logEventInfo.TimeStamp.ToString(CultureInfo.InvariantCulture);

        SetupColors(logEventInfo);
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public LoggingEvent Info { get; }


    public string Time { get; }
    public string LoggerName { get; }
    public string Level { get; }
    public string FormattedMessage { get; }
    public Exception? Exception { get; }
    // ReSharper disable MemberCanBePrivate.Global
    public string ToolTip { get; }
    public SolidColorBrush? Background { get; private set; }
    public SolidColorBrush? Foreground { get; private set; }
    public SolidColorBrush? BackgroundMouseOver { get; private set; }
    public SolidColorBrush? ForegroundMouseOver { get; private set; }

    private void SetupColors(LogEventInfo logEventInfo)
    {
        if (logEventInfo.Level == LogLevel.Warn)
        {
            Background = Brushes.DarkOrange;
            BackgroundMouseOver = Brushes.DarkGoldenrod;
        }
        else if (logEventInfo.Level == LogLevel.Error || logEventInfo.Level == LogLevel.Fatal)
        {
            Background = Brushes.DarkRed;
            BackgroundMouseOver = Brushes.DarkRed;
        }
        else
        {
            Background = Brushes.Black;
            BackgroundMouseOver = Brushes.DarkGray;
        }

        Foreground = Brushes.White;
        ForegroundMouseOver = Brushes.White;
    }

    private class RenderHelper : TargetWithLayout
    {
        internal string Render(LogEventInfo evt) => RenderLogEvent(Layout, evt);

        internal string GetLoggerName(LogEventInfo evt)
            => evt.LoggerName ?? evt.CallerClassName;
    }
}