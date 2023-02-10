using System.Collections.Immutable;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using Tauron.Application;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

internal sealed partial class DummyOperator : IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly ILogger<DummyOperator> _logger;
    private readonly DeviceId _deviceId;
    
    private readonly Action<DeviceButton, bool> _stateChange;
    private readonly Action<DeviceSensor, DeviceManagerMessages.ISensorBox> _valueChange;

    private readonly Channel<LogData> _logData;
    private readonly LogCollector<LogData> _currentLog = new(
        "Dummy_Operator",
        TauronEnviroment.LoggerFactory.CreateLogger<LogCollector<LogData>>(),
        e => e);
    private ButtonSensorPair[] _pairs;


    internal DummyOperator(
        DeviceId deviceId,
        Action<DeviceButton, bool> stateChange,
        Action<DeviceSensor, DeviceManagerMessages.ISensorBox> valueChange,
        ILogger<DummyOperator> logger)
    {
        _pairs = Array.Empty<ButtonSensorPair>();
        _deviceId = deviceId;
        _stateChange = stateChange;
        _valueChange = valueChange;
        _logger = logger;
        _logData = Channel.CreateBounded<LogData>(new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });
    }

    public void Dispose()
    {
        if(_cancellation.IsCancellationRequested) return;

        _cancellation.Cancel();
    }

    internal void Init(ButtonSensorPair[] pairs)
    {
        _pairs = pairs;
        Task.Factory.StartNew(Simulation, TaskCreationOptions.LongRunning).Ignore();
    }

    [LoggerMessage(68, LogLevel.Critical, "Error on Run Device Simulation")]
    private partial void CriticalSimulationError(Exception error);

    // ReSharper disable once CognitiveComplexity
    private async Task Simulation()
    {
        _currentLog.CollectLogs(_logData.Reader);
        try
        {
            while (!_cancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), _cancellation.Token).ConfigureAwait(false);

                foreach (var sensorPair in _pairs)
                    UpdatePair(sensorPair);

                _logData.Writer.TryWrite(
                    new LogData(
                        LogLevel.Information,
                        LogCategory.From("Test"),
                        SimpleMessage.From("TestLog"),
                        DateTime.Now,
                        ImmutableDictionary<string, PropertyValue>.Empty
                           .Add("Test", PropertyValue.From("TestValue"))));
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            CriticalSimulationError(e);
        }

        _logData.Writer.TryComplete();
        _cancellation.Dispose();
    }

    private void UpdatePair(ButtonSensorPair sensorPair)
    {
        if(sensorPair.Clicked)
        {
            sensorPair.Counter = 0;
            sensorPair.Clicked = false;
            sensorPair.CurrentState = true;

            _stateChange(sensorPair.Button, arg2: false);
            return;
        }

        if(!sensorPair.CurrentState)
            return;

        sensorPair.Counter++;
        sensorPair.SensorValue++;
        sensorPair.CurrentValue = DeviceManagerMessages.SensorBox.Create($"Sensor Value: {sensorPair.SensorValue}");

        _valueChange(sensorPair.Sensor, sensorPair.CurrentValue);

        if(sensorPair.Counter <= 5)
            return;

        sensorPair.CurrentState = false;
        _stateChange(sensorPair.Button, arg2: true);
    }

    internal Task<LogBatch> NextBatch()
        => _currentLog.GetLogs(_deviceId);

    internal void ApplyClick(DeviceId id)
        => _pairs.First(p => p.Button.Identifer == id).Clicked = true;
}