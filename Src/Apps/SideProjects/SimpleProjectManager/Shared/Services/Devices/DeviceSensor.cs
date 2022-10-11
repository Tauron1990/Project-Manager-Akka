namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public enum SensorType
{
    None = 0,
    Double,
    String,
    Number
}

public sealed record DeviceSensor(string DisplayName, string Identifer, SensorType SensorType);