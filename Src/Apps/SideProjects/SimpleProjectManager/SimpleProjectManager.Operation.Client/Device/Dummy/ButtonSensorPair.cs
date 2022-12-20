using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

internal sealed record ButtonSensorPair(CategoryName Category, DeviceButton Button, DeviceSensor Sensor)
{
    internal bool CurrentState { get; set; }

    internal bool Clicked { get; set; }
    
    internal DeviceManagerMessages.ISensorBox CurrentValue { get; set; } = DeviceManagerMessages.SensorBox.CreateDefault(Sensor.SensorType);

    internal int SensorValue { get; set; }

    internal int Counter { get; set; }
}