using System.Collections.Immutable;
using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

public class DummyMachine : IMachine, IDisposable
{
    private readonly DummyOperator _dummyOperator;
    
    private ButtonSensorPair[] _pairs = Array.Empty<ButtonSensorPair>();
    private ImmutableDictionary<string, Action<bool>> _stateChanges = ImmutableDictionary<string, Action<bool>>.Empty;
    private ImmutableDictionary<string, DeviceManagerMessages.ISensorBox> _sensorBoxes = ImmutableDictionary<string, DeviceManagerMessages.ISensorBox>.Empty;

    public DummyMachine()
    {
        _dummyOperator = new DummyOperator(StateChange, ValueChange);
    }

    private void StateChange(DeviceButton button, bool state)
    {
        if(_stateChanges.TryGetValue(button.Identifer, out var action))
            action(state);
    }

    private void ValueChange(DeviceSensor sensor, DeviceManagerMessages.ISensorBox sensorBox)
        => _sensorBoxes = _sensorBoxes.SetItem(sensor.Identifer, sensorBox);

    private static string Id()
        => Guid.NewGuid().ToString("N");
    
    public Task Init()
    {
        _pairs = new[]
                 {
                     new ButtonSensorPair(
                         "Test",
                         new DeviceButton("TestButton1", Id()),
                         new DeviceSensor("TestValue1", Id(), SensorType.String)),
                     new ButtonSensorPair(
                         "Test",
                         new DeviceButton("TestButton2", Id()),
                         new DeviceSensor("TestValue2", Id(), SensorType.String)),
                     new ButtonSensorPair(
                         "Test1",
                         new DeviceButton("TestButton3", Id()),
                         new DeviceSensor("TestValue3", Id(), SensorType.String)),
                     new ButtonSensorPair(
                         "Test1",
                         new DeviceButton("TestButton4", Id()),
                         new DeviceSensor("TestValue4", Id(), SensorType.String)),
                     new ButtonSensorPair(
                         "Test2",
                         new DeviceButton("TestButton5", Id()),
                         new DeviceSensor("TestValue5", Id(), SensorType.String)),
                     new ButtonSensorPair(
                         "Test2",
                         new DeviceButton("TestButton6", Id()),
                         new DeviceSensor("TestValue6", Id(), SensorType.String))
                 };
        
        _dummyOperator.Init(_pairs);
        return Task.CompletedTask;
    }

    private DeviceUiGroup CreateGroup()
    {
        var root = new DeviceUiGroup(ImmutableList<DeviceUiGroup>.Empty, ImmutableList<DeviceSensor>.Empty, ImmutableList<DeviceButton>.Empty);
        
        foreach (var grouping in _pairs.GroupBy(p => p.Category))
        {
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
        }

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
    
    public Task<DeviceInformations> CollectInfo()
        => Task.FromResult(new DeviceInformations(true, CreateGroup()));

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => Task.FromResult(_sensorBoxes.GetValueOrDefault(sensor.Identifer) ?? DeviceManagerMessages.SensorBox.CreateDefault(sensor.SensorType));

    public void ButtonClick(string identifer)
        => _dummyOperator.ApplyClick(identifer);

    public void WhenButtonStateChanged(string identifer, Action<bool> onButtonStateChanged)
        => _stateChanges = _stateChanges.SetItem(identifer, onButtonStateChanged);

    public Task<LogBatch> NextLogBatch()
        => Task.FromResult(_dummyOperator.NextBatch());

    public void Dispose()
        => _dummyOperator.Dispose();
}