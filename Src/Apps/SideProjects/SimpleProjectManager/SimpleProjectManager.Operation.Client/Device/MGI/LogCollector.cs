using System.Collections.Immutable;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.MGI;

internal sealed partial class LogCollector
{
    private readonly ILogger<LogCollector> _logger;
    private readonly List<LogData> _pending = new();
    private readonly SemaphoreSlim _sync = new(1, 1);

    private int _errorCount;
    
    internal LogCollector(ILogger<LogCollector> logger)
    {
        _logger = logger;

    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error On Read Logs")]
    private partial void LogReadError(Exception e);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Critical Error On Read Logs: {fromReadError}")]
    private partial void LogCrticalError(Exception e, bool fromReadError);

    internal async void CollectLogs(ChannelReader<LogInfo> log)
    {
        if(log.Completion.IsCompleted)
            return;
        
        try
        {
            await foreach (LogInfo element in log.ReadAllAsync().ConfigureAwait(false))
            {
                await _sync.WaitAsync().ConfigureAwait(false);
                
                try
                {
                    _pending.Add(
                        new LogData(
                            LogLevel.Trace,
                            LogCategory.From(element.Type),
                            SimpleMessage.From(element.Content),
                            element.TimeStamp,
                            ImmutableDictionary<string, PropertyValue>.Empty
                               .Add("Application", PropertyValue.From(element.Application))));
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
}