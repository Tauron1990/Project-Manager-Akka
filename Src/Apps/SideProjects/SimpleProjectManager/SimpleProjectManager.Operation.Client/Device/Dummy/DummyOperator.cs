﻿using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

internal sealed partial class DummyOperator : IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly ILogger<DummyOperator> _logger;

    private readonly Action<DeviceButton, bool> _stateChange;
    private readonly Action<DeviceSensor, DeviceManagerMessages.ISensorBox> _valueChange;
    private ImmutableList<LogData> _currentLog = ImmutableList<LogData>.Empty;
    private ButtonSensorPair[] _pairs;


    internal DummyOperator(
        Action<DeviceButton, bool> stateChange,
        Action<DeviceSensor, DeviceManagerMessages.ISensorBox> valueChange,
        ILogger<DummyOperator> logger)
    {
        _pairs = Array.Empty<ButtonSensorPair>();
        _stateChange = stateChange;
        _valueChange = valueChange;
        _logger = logger;

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
        try
        {
            while (!_cancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), _cancellation.Token).ConfigureAwait(false);

                foreach (ButtonSensorPair sensorPair in _pairs)
                    UpdatePair(sensorPair);

                #pragma warning disable GU0011
                Interlocked.Exchange(
                    ref _currentLog,
                    _currentLog.Add(
                        new LogData(
                            LogLevel.Information,
                            LogCategory.From("Test"),
                            SimpleMessage.From("TestLog"),
                            DateTime.Now,
                            ImmutableDictionary<PropertyName, PropertyValue>.Empty
                               .Add(PropertyName.From("Test"), PropertyValue.From("TestValue")))));
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            CriticalSimulationError(e);
        }

        _cancellation.Dispose();
    }

    private void UpdatePair(ButtonSensorPair sensorPair)
    {
        if(sensorPair.Clicked)
        {
            sensorPair.Counter = 0;
            sensorPair.Clicked = false;
            sensorPair.CurrentState = true;

            _stateChange(sensorPair.Button, false);
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
        _stateChange(sensorPair.Button, true);
    }

    internal LogBatch NextBatch()
    {
        var current = Interlocked.Exchange(ref _currentLog, ImmutableList<LogData>.Empty);

        return new LogBatch(current);
    }

    internal void ApplyClick(DeviceId id)
        => _pairs.First(p => p.Button.Identifer == id).Clicked = true;
}