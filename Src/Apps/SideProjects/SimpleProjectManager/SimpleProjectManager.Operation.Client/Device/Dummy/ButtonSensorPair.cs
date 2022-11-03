using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Dummy;

internal sealed record ButtonSensorPair(string Category, DeviceButton Button, DeviceSensor Sensor)
{
    public bool CurrentState { get; set; }
    
    public bool Clicked { get; set; }
    
    public DeviceManagerMessages.ISensorBox CurrentValue { get; set; } = DeviceManagerMessages.SensorBox.CreateDefault(Sensor.SensorType);

    public int SensorValue { get; set; }
    
    public int Counter { get; set; }
}