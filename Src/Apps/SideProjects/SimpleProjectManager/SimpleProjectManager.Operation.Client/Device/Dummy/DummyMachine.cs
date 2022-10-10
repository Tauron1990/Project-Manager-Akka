using System.Collections.Immutable;
using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

public class DummyMachine : IMachine
{
    private sealed record ButtonSensorPair(string Category, DeviceButton Button, DeviceSensor Sensor);

    private ImmutableDictionary<string, ButtonSensorPair> _pairs = ImmutableDictionary<string, ButtonSensorPair>.Empty;

    public Task Init()
    {
        return Task.CompletedTask;
    }

    public Task<DeviceInformations> CollectInfo()
        => throw new NotImplementedException();

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => throw new NotImplementedException();

    public void ButtonClick(string identifer)
    {
        throw new NotImplementedException();
    }

    public void WhenButtonStateChanged(string identifer, Action<bool> onButtonStateChanged)
    {
        throw new NotImplementedException();
    }
}