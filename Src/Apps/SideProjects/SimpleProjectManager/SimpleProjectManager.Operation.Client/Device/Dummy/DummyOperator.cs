using System.Collections.Immutable;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

internal sealed class DummyOperator : IDisposable
{
    private ButtonSensorPair[] _pairs;
    private ImmutableList<LogData> _currentLog = ImmutableList<LogData>.Empty;

    private readonly Action<DeviceButton, bool> _stateChange;
    private readonly Action<DeviceSensor, DeviceManagerMessages.ISensorBox> _valueChange;
    private readonly CancellationTokenSource _cancellation = new();

    public DummyOperator(Action<DeviceButton, bool> stateChange, Action<DeviceSensor, DeviceManagerMessages.ISensorBox> valueChange)
    {
        _pairs = Array.Empty<ButtonSensorPair>();
        _stateChange = stateChange;
        _valueChange = valueChange;

    }

    public void Init(ButtonSensorPair[] pairs)
    {
        _pairs = pairs;
        Task.Factory.StartNew(Simulation, TaskCreationOptions.LongRunning);
    }

    // ReSharper disable once CognitiveComplexity
    private async Task Simulation()
    {
        try
        {
            while (!_cancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                foreach (var sensorPair in _pairs)
                {
                    if(sensorPair.CurrentState)
                    {
                        if(!sensorPair.Clicked)
                            continue;

                        sensorPair.Clicked = false;
                        sensorPair.CurrentState = false;
                        sensorPair.SensorValue++;
                        sensorPair.CurrentValue = DeviceManagerMessages.SensorBox.Create($"Sernsor Value: {sensorPair.SensorValue}");

                        _valueChange(sensorPair.Sensor, sensorPair.CurrentValue);
                    }
                    else
                    {
                        sensorPair.Counter++;

                        if(sensorPair.Counter <= 5)
                            continue;

                        sensorPair.Counter = 0;
                        sensorPair.Clicked = false;
                        sensorPair.CurrentState = true;

                        _stateChange(sensorPair.Button, sensorPair.CurrentState);
                    }
                }

                Interlocked.Exchange(
                    ref _currentLog, 
                    _currentLog.Add(new LogData("TestLog", DateTime.Now, ImmutableDictionary<string, string>.Empty.Add("Test", "TestValue"))));
            }
        }
        catch (OperationCanceledException) { }

        _cancellation.Dispose();
    }
    
    public LogBatch NextBatch()
    {
        var current = Interlocked.Exchange(ref _currentLog, ImmutableList<LogData>.Empty);

        return new LogBatch(string.Empty, current);
    }

    public void ApplyClick(string id)
        => _pairs.First(p => p.Button.Identifer == id).Clicked = true;

    public void Dispose()
    {
        if(_cancellation.IsCancellationRequested) return;
        _cancellation.Cancel();
    }
}