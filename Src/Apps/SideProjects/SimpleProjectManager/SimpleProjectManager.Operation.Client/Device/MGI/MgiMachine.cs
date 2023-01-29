using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Operation.Client.Device.MGI;

public sealed class MgiMachine : IMachine
{
    public Task Init()
        => throw new NotImplementedException();

    public Task<DeviceInformations> CollectInfo()
        => throw new NotImplementedException();

    public IState<DeviceUiGroup>? UIUpdates()
        => throw new NotImplementedException();

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => throw new NotImplementedException();

    public void ButtonClick(DeviceId identifer)
    {
        throw new NotImplementedException();
    }

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
    {
        throw new NotImplementedException();
    }

    public Task<LogBatch> NextLogBatch()
        => throw new NotImplementedException();
}