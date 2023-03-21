using System.Collections.Immutable;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class DummyMachine : IMachine, IDisposable
{
    private const string DummyId = "9E72CF8B-B33E-421E-BFB5-DB17D70379D2";

    private readonly (string Name, DeviceId Id)[] _buttons =
    {
        ("TestButton1", DeviceId.ForName("TestButton1")),
        ("TestButton2", DeviceId.ForName("TestButton2")),
        ("TestButton3", DeviceId.ForName("TestButton3")),
        ("TestButton4", DeviceId.ForName("TestButton4")),
        ("TestButton5", DeviceId.ForName("TestButton5")),
        ("TestButton6", DeviceId.ForName("TestButton6")),
    };

    private readonly DeviceId _deviceId;

    private readonly DummyOperator _dummyOperator;

    private ButtonSensorPair[] _pairs = Array.Empty<ButtonSensorPair>();
    private ImmutableDictionary<DeviceId, DeviceManagerMessages.ISensorBox> _sensorBoxes = ImmutableDictionary<DeviceId, DeviceManagerMessages.ISensorBox>.Empty;
    private ImmutableDictionary<DeviceId, Action<bool>> _stateChanges = ImmutableDictionary<DeviceId, Action<bool>>.Empty;

    public DummyMachine(ILoggerFactory loggerFactory, OperationConfiguration operationConfiguration)
    {
        _deviceId = operationConfiguration.CreateDeviceId(DummyId);
        _dummyOperator = new DummyOperator(_deviceId, StateChange, ValueChange, loggerFactory.CreateLogger<DummyOperator>());
    }

    public void Dispose()
        => _dummyOperator.Dispose();

    public Task Init(IActorContext context)
    {
        _pairs = new[]
                 {
                     new ButtonSensorPair(
                         CategoryName.From("Test"),
                         new DeviceButton(DisplayName.From(_buttons[0].Name), _buttons[0].Id),
                         new DeviceSensor(DisplayName.From("TestValue1"), DeviceId.ForName("TestValue1"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test"),
                         new DeviceButton(DisplayName.From(_buttons[1].Name), _buttons[1].Id),
                         new DeviceSensor(DisplayName.From("TestValue2"), DeviceId.ForName("TestValue2"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test1"),
                         new DeviceButton(DisplayName.From(_buttons[2].Name), _buttons[2].Id),
                         new DeviceSensor(DisplayName.From("TestValue3"), DeviceId.ForName("TestValue3"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test1"),
                         new DeviceButton(DisplayName.From(_buttons[3].Name), _buttons[3].Id),
                         new DeviceSensor(DisplayName.From("TestValue4"), DeviceId.ForName("TestValue4"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test2"),
                         new DeviceButton(DisplayName.From(_buttons[4].Name), _buttons[4].Id),
                         new DeviceSensor(DisplayName.From("TestValue5"), DeviceId.ForName("TestValue5"), SensorType.String)),
                     new ButtonSensorPair(
                         CategoryName.From("Test2"),
                         new DeviceButton(DisplayName.From(_buttons[5].Name), _buttons[5].Id),
                         new DeviceSensor(DisplayName.From("TestValue6"), DeviceId.ForName("TestValue6"), SensorType.String)),
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
                _buttons.Select(p => p.Id).Select(id => new ButtonState(id, State: true)).ToImmutableList(),
                ActorRefs.Nobody));

    public IState<DeviceUiGroup>? UIUpdates()
        => null;

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => Task.FromResult(_sensorBoxes.GetValueOrDefault(sensor.Identifer) ?? DeviceManagerMessages.SensorBox.CreateDefault(SensorType.String));

    public void ButtonClick(DeviceId identifer)
        => _dummyOperator.ApplyClick(identifer);

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
        => _stateChanges = _stateChanges.SetItem(identifer, onButtonStateChanged);

    public Task<LogBatch> NextLogBatch()
        => _dummyOperator.NextBatch();

    private void StateChange(DeviceButton button, bool state)
    {
        if(_stateChanges.TryGetValue(button.Identifer, out var action))
            action(state);
    }

    private void ValueChange(DeviceSensor sensor, DeviceManagerMessages.ISensorBox sensorBox)
        => _sensorBoxes = _sensorBoxes.SetItem(sensor.Identifer, sensorBox);

    private DeviceUiGroup CreateGroup()
    {
        DeviceUiGroup ui = DeviceUi.Tab(
            "Root",
            _pairs.GroupBy(p => p.Category)
               .Select(
                    p => DeviceUi.GroupVertical(
                        p.Key.Value, 
                        p.SelectMany(GetPairUi)))
        );
            
        IEnumerable<DeviceUiGroup> GetPairUi(ButtonSensorPair arg)
        {
            yield return DeviceUi.Sensor(arg.Sensor);
            yield return DeviceUi.Button(arg.Button);
        }

        return ui;
    }
}