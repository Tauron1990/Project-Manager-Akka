using System.Collections.Immutable;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

public sealed class DummyMachine : IMachine, IDisposable
{
    private readonly DummyOperator _dummyOperator;
    private readonly DeviceId _deviceId = DeviceId.New;
        
    private ButtonSensorPair[] _pairs = Array.Empty<ButtonSensorPair>();
    private ImmutableDictionary<DeviceId, DeviceManagerMessages.ISensorBox> _sensorBoxes = ImmutableDictionary<DeviceId, DeviceManagerMessages.ISensorBox>.Empty;
    private ImmutableDictionary<DeviceId, Action<bool>> _stateChanges = ImmutableDictionary<DeviceId, Action<bool>>.Empty;

    public DummyMachine(ILoggerFactory loggerFactory)
        => _dummyOperator = new DummyOperator(StateChange, ValueChange, loggerFactory.CreateLogger<DummyOperator>());

    public void Dispose()
        => _dummyOperator.Dispose();

    public Task Init()
    {
        _pairs = new[]
                 {
                     new ButtonSensorPair(
                         CategoryName.From("Test"),
                         new DeviceButton(DisplayName.From("TestButton1"), Id()),
                         new DeviceSensor(DisplayName.From("TestValue1"), Id(), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test"),
                         new DeviceButton(DisplayName.From("TestButton2"), Id()),
                         new DeviceSensor(DisplayName.From("TestValue2"), Id(), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test1"),
                         new DeviceButton(DisplayName.From("TestButton3"), Id()),
                         new DeviceSensor(DisplayName.From("TestValue3"), Id(), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test1"),
                         new DeviceButton(DisplayName.From("TestButton4"), Id()),
                         new DeviceSensor(DisplayName.From("TestValue4"), Id(), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test2"),
                         new DeviceButton(DisplayName.From("TestButton5"), Id()),
                         new DeviceSensor(DisplayName.From("TestValue5"), Id(), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test2"),
                         new DeviceButton(DisplayName.From("TestButton6"), Id()),
                         new DeviceSensor(DisplayName.From("TestValue6"), Id(), SensorType.String))
                 };

        _dummyOperator.Init(_pairs);

        return Task.CompletedTask;
    }

    public Task<DeviceInformations> CollectInfo()
        => Task.FromResult(new DeviceInformations(_deviceId, DeviceName.From("Dummy Operator"), HasLogs: true, CreateGroup(), ActorRefs.Nobody));

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => Task.FromResult(_sensorBoxes.GetValueOrDefault(sensor.Identifer) ?? DeviceManagerMessages.SensorBox.CreateDefault(sensor.SensorType));

    public void ButtonClick(DeviceId identifer)
        => _dummyOperator.ApplyClick(identifer);

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
        => _stateChanges = _stateChanges.SetItem(identifer, onButtonStateChanged);

    public Task<LogBatch> NextLogBatch()
        => Task.FromResult(_dummyOperator.NextBatch());

    private void StateChange(DeviceButton button, bool state)
    {
        if(_stateChanges.TryGetValue(button.Identifer, out var action))
            action(state);
    }

    private void ValueChange(DeviceSensor sensor, DeviceManagerMessages.ISensorBox sensorBox)
        => _sensorBoxes = _sensorBoxes.SetItem(sensor.Identifer, sensorBox);

    private static DeviceId Id()
        => DeviceId.New;

    private DeviceUiGroup CreateGroup()
    {
        var root = new DeviceUiGroup(ImmutableList<DeviceUiGroup>.Empty, ImmutableList<DeviceSensor>.Empty, ImmutableList<DeviceButton>.Empty);

        foreach (var grouping in _pairs.GroupBy(p => p.Category))
            if(grouping.Key == "Test")
                root = Fill(root, grouping);
            else
                root = root with
                       {
                           Groups = root.Groups.Add(
                               Fill(
                                   new DeviceUiGroup(
                                       ImmutableList<DeviceUiGroup>.Empty,
                                       ImmutableList<DeviceSensor>.Empty,
                                       ImmutableList<DeviceButton>.Empty),
                                   grouping))
                       };

        return root;

        DeviceUiGroup Fill(DeviceUiGroup uiGroup, IEnumerable<ButtonSensorPair> pairs)
            => pairs.Aggregate(
                uiGroup,
                (current, sensorPair) => current with
                                         {
                                             DeviceButtons = current.DeviceButtons.Add(sensorPair.Button),
                                             Sensors = current.Sensors.Add(sensorPair.Sensor)
                                         });
    }
}