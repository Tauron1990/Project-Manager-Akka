using System.Collections.Immutable;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

internal sealed class DummyOperator : IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();

    private readonly DeviceId _name;
    private readonly Action<DeviceButton, bool> _stateChange;
    private readonly Action<DeviceSensor, DeviceManagerMessages.ISensorBox> _valueChange;
    private ImmutableList<LogData> _currentLog = ImmutableList<LogData>.Empty;
    private ButtonSensorPair[] _pairs;

    public DummyOperator(DeviceId name, Action<DeviceButton, bool> stateChange, Action<DeviceSensor, DeviceManagerMessages.ISensorBox> valueChange)
    {
        _pairs = Array.Empty<ButtonSensorPair>();
        _name = name;
        _stateChange = stateChange;
        _valueChange = valueChange;

    }

    public void Dispose()
    {
        if(_cancellation.IsCancellationRequested) return;

        _cancellation.Cancel();
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

                foreach (ButtonSensorPair sensorPair in _pairs)
                    UpdatePair(sensorPair);

                Interlocked.Exchange(
                    ref _currentLog,
                    _currentLog.Add(new LogData(
                        SimpleMessage.From("TestLog"), 
                        DateTime.Now, 
                        ImmutableDictionary<PropertyName, PropertyValue>.Empty
                           .Add(PropertyName.From("Test"), PropertyValue.From("TestValue")))));
            }
        }
        catch (OperationCanceledException) { }

        _cancellation.Dispose();
    }

    private void UpdatePair(ButtonSensorPair sensorPair)
    {
        if(sensorPair.CurrentState)
        {
            if(!sensorPair.Clicked)
                return;

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
                return;

            sensorPair.Counter = 0;
            sensorPair.Clicked = false;
            sensorPair.CurrentState = true;

            _stateChange(sensorPair.Button, sensorPair.CurrentState);
        }
    }

    public LogBatch NextBatch()
    {
        var current = Interlocked.Exchange(ref _currentLog, ImmutableList<LogData>.Empty);

        return new LogBatch(current);
    }

    public void ApplyClick(DeviceId id)
        => _pairs.First(p => p.Button.Identifer == id).Clicked = true;
}