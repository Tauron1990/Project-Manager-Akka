using System.Collections.Immutable;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

public sealed class DummyMachine : IMachine, IDisposable
{
    private const string DummyId = "9E72CF8B-B33E-421E-BFB5-DB17D70379D2";
    
    private readonly DummyOperator _dummyOperator;
    private readonly DeviceId _deviceId;

    private readonly string[] _buttons = {
                                             "TestButton1",
                                             "TestButton2",
                                             "TestButton3",
                                             "TestButton4",
                                             "TestButton5",
                                             "TestButton6",
                                         };
    
    private ButtonSensorPair[] _pairs = Array.Empty<ButtonSensorPair>();
    private ImmutableDictionary<DeviceId, DeviceManagerMessages.ISensorBox> _sensorBoxes = ImmutableDictionary<DeviceId, DeviceManagerMessages.ISensorBox>.Empty;
    private ImmutableDictionary<DeviceId, Action<bool>> _stateChanges = ImmutableDictionary<DeviceId, Action<bool>>.Empty;

    public DummyMachine(ILoggerFactory loggerFactory, OperationConfiguration operationConfiguration)
    {
        _deviceId = DeviceId.ForName($"{DummyId}--{operationConfiguration.Name.Value}");
        _dummyOperator = new DummyOperator(StateChange, ValueChange, loggerFactory.CreateLogger<DummyOperator>());
    }

    public void Dispose()
        => _dummyOperator.Dispose();

    public Task Init()
    {
        _pairs = new[]
                 {
                     new ButtonSensorPair(
                         CategoryName.From("Test"),
                         new DeviceButton(DisplayName.From(_buttons[0]), Id(_buttons[0])),
                         new DeviceSensor(DisplayName.From("TestValue1"), Id("TestValue1"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test"),
                         new DeviceButton(DisplayName.From(_buttons[1]), Id(_buttons[1])),
                         new DeviceSensor(DisplayName.From("TestValue2"), Id("TestValue2"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test1"),
                         new DeviceButton(DisplayName.From(_buttons[2]), Id(_buttons[2])),
                         new DeviceSensor(DisplayName.From("TestValue3"), Id("TestValue3"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test1"),
                         new DeviceButton(DisplayName.From(_buttons[3]), Id(_buttons[3])),
                         new DeviceSensor(DisplayName.From("TestValue4"), Id("TestValue4"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test2"),
                         new DeviceButton(DisplayName.From(_buttons[4]), Id(_buttons[4])),
                         new DeviceSensor(DisplayName.From("TestValue5"), Id("TestValue5"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test2"),
                         new DeviceButton(DisplayName.From(_buttons[5]), Id(_buttons[5])),
                         new DeviceSensor(DisplayName.From("TestValue6"), Id("TestValue6"), SensorType.String)),
                 };

        _dummyOperator.Init(_pairs);

        return Task.CompletedTask;
    }

    public Task<DeviceInformations> CollectInfo()
        => Task.FromResult(
            new DeviceInformations(
                _deviceId,
                DeviceName.From("Dummy Operator"),
                HasLogs: true,
                CreateGroup(),
                _buttons.Select(DeviceId.ForName).Select(id => new ButtonState(id, State: true)).ToImmutableList(),
                ActorRefs.Nobody));

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => Task.FromResult(_sensorBoxes.GetValueOrDefault(sensor.Identifer) ?? DeviceManagerMessages.SensorBox.CreateDefault(SensorType.String));

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

    private static DeviceId Id(string name)
        => DeviceId.ForName(name);

    private DeviceUiGroup CreateGroup()
    {
        var root = new DeviceUiGroup("Root", ImmutableList<DeviceUiGroup>.Empty, ImmutableList<DeviceSensor>.Empty, ImmutableList<DeviceButton>.Empty);

        foreach (var grouping in _pairs.GroupBy(p => p.Category))
            if(grouping.Key == "Test")
                root = Fill(root, grouping);
            else
                root = root with
                       {
                           Groups = root.Groups.Add(
                               Fill(
                                   new DeviceUiGroup(
                                       grouping.Key.Value,
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
                                             Sensors = current.Sensors.Add(sensorPair.Sensor),
                                         });
    }
}