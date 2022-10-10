using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Operation.Client.Device;

public interface IMachine
{
    Task Init();

    Task<DeviceInformations> CollectInfo();

    Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor);
}