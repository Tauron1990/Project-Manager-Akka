namespace SimpleProjectManager.Shared.Services.Devices;

public enum SensorType
{
    None = 0,
    Double,
    String,
    Number
}

public sealed record DeviceSensor(DisplayName DisplayName, DeviceId Identifer, SensorType SensorType);