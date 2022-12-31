﻿using System.Collections.Immutable;
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
        _deviceId = DeviceId.ForName($"{DummyId}--{operationConfiguration.Name.Value}");
        _dummyOperator = new DummyOperator(_deviceId, StateChange, ValueChange, loggerFactory.CreateLogger<DummyOperator>());
    }

    public void Dispose()
        => _dummyOperator.Dispose();

    public Task Init()
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
                                   grouping)),
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