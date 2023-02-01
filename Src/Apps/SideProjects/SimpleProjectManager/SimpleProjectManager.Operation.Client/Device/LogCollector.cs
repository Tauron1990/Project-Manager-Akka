using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using NLog;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Application.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SimpleProjectManager.Operation.Client.Device;

internal sealed partial class LogCollector<TSource>
{
    private readonly string _appName;
    private readonly ILogger<LogCollector<TSource>> _logger;
    private readonly Func<TSource, LogData> _converter;
    private readonly List<LogData> _pending = new();
    private readonly SemaphoreSlim _sync = new(1, 1);
    private readonly Action<LogData> _localLogger;

    private int _errorCount;
    
    internal LogCollector(string appName, ILogger<LogCollector<TSource>> logger, Func<TSource, LogData> converter)
    {
        _appName = appName;
        _logger = logger;
        _converter = converter;

        _localLogger = CreateLocalLogger();
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error On Read Logs")]
    private partial void LogReadError(Exception e);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Critical Error On Read Logs: {fromReadError}")]
    private partial void LogCrticalError(Exception e, bool fromReadError);

    internal async void CollectLogs(ChannelReader<TSource> log)
    {
        if(log.Completion.IsCompleted)
            return;
        
        try
        {
            await foreach (TSource element in log.ReadAllAsync().ConfigureAwait(false))
            {
                await _sync.WaitAsync().ConfigureAwait(false);
                
                try
                {
                    LogData data = _converter(element);
                    if(_pending.Count > 1000)
                        _pending.RemoveAt(0);
                    _pending.Add(data);
                    _localLogger(data);
                }
                finally
                {
                    _sync.Release();
                }
            }
        }
        catch (Exception e)
        {
            Interlocked.Increment(ref _errorCount);

            if(_errorCount > 5)
            {
                LogReadError(e);
                CollectLogs(log);
            }
            else
                LogCrticalError(e, fromReadError: false);
        }
    }

    internal async Task<LogBatch> GetLogs(DeviceId deviceId)
    {
        await _sync.WaitAsync().ConfigureAwait(false);
        
        try
        {
            var logs = _pending.ToImmutableList();
            _pending.Clear();

            return new LogBatch(deviceId, logs);
        }
        finally
        {
            _sync.Release();
        }
    }
    
    private Action<LogData> CreateLocalLogger()
    {
        LogFactory fac = CreateFactory();

        return d =>
               {
                   Logger logger = fac.GetLogger(d.Category.Value);
                   LogEventBuilder builder;

                   switch (d.LogLevel)
                   {
                       case LogLevel.Trace:
                           builder = logger.ForTraceEvent();
                           break;
                       case LogLevel.Debug:
                           builder = logger.ForDebugEvent();
                           break;
                       case LogLevel.None:
                       case LogLevel.Information:
                           builder = logger.ForInfoEvent();
                           break;
                       case LogLevel.Warning:
                           builder = logger.ForWarnEvent();
                           break;
                       case LogLevel.Error:
                           builder = logger.ForErrorEvent();
                           break;
                       case LogLevel.Critical:
                           builder = logger.ForFatalEvent();
                           break;
                       default:
                           throw new UnreachableException("Log Level");
                   }

                   foreach (var data in d.Data)
                       builder.Property(data.Key, data.Value);

                   builder.TimeStamp(d.Occurance);
                   builder.Message(d.Message.Value);
                   
                   builder.Log();
               };
    }

    private LogFactory CreateFactory()
    {
        return new LogFactory().Setup(s => s.ConfigurateFile(_appName));
    }
}