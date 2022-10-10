using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Operation.Client.Device;

public interface IMachine
{
    Task Init();

    Task<DeviceInformations> CollectInfo();

    Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor);
    void ButtonClick(string identifer);
    void WhenButtonStateChanged(string identifer, Action<bool> onButtonStateChanged);
}