using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Operation.Client.Device;

public interface IMachine
{
    Task Init();

    Task<DeviceInformations> CollectInfo();

    IState<DeviceUiGroup>? UIUpdates();

    Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor);
    void ButtonClick(DeviceId identifer);
    void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged);
    Task<LogBatch> NextLogBatch();
}