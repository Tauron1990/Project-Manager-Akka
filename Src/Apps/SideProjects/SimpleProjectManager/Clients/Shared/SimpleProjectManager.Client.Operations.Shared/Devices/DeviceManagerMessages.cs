using System.Collections.Immutable;
using JetBrains.Annotations;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public static class DeviceManagerMessages
{
    public sealed record DeviceInfoResponse(bool Duplicate, string? Error);

    public interface IDeviceCommand
    {
        string DeviceName { get; }
    }

    public sealed record UpdateSensor(string DeviceName, string Identifer, ISensorBox SensorValue) : IDeviceCommand;

    public interface ISensorBox
    {
        SensorType SensorType { get; }

        string AsString { get; }

        int AsInt { get; }

        double AsDouble { get; }
    }

    [PublicAPI]
    public static class SensorBox
    {
        public static ISensorBox CreateDefault(SensorType type)
            => type switch
            {

                SensorType.Double => Create(0d),
                SensorType.String => Create(string.Empty),
                SensorType.Number => Create(0),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };


        public static ISensorBox Create(string value)
            => new StringSensorBox(value, SensorType.String);

        public static ISensorBox Create(int value)
            => new NumberSensorBox(value, SensorType.Number);

        public static ISensorBox Create(double value)
            => new DoubleSensorBox(value, SensorType.Double);
    }

    [UsedImplicitly]
    public sealed record StringSensorBox(string Value, SensorType SensorType) : ISensorBox
    {
        public string AsString => Value;

        public int AsInt => throw new InvalidOperationException("Wrong BoxType");

        public double AsDouble => throw new InvalidOperationException("Wrong BoxType");
    }

    [UsedImplicitly]
    public sealed record NumberSensorBox(int Value, SensorType SensorType) : ISensorBox
    {
        public string AsString => throw new InvalidOperationException("Wrong BoxType");

        public int AsInt => Value;

        public double AsDouble => throw new InvalidOperationException("Wrong BoxType");
    }

    [UsedImplicitly]
    public sealed record DoubleSensorBox(double Value, SensorType SensorType) : ISensorBox
    {
        public string AsString => throw new InvalidOperationException("Wrong BoxType");

        public int AsInt => throw new InvalidOperationException("Wrong BoxType");

        public double AsDouble => Value;
    }

    public sealed record QuerySensorValue(string DeviceName, string Identifer) : IDeviceCommand;

    public sealed record SensorValueResult(string? Error, ISensorBox? Value);

    public sealed record QueryButtonState(string DeviceName, string Identifer) : IDeviceCommand;

    public sealed record UpdateButtonState(string DeviceName, string Identifer, bool State) : IDeviceCommand;

    public sealed record ButtonClick(string DeviceName, string Identifer) : IDeviceCommand;

    public sealed record ButtonStateResponse(bool CanClick);

    public sealed record QueryLoggerBatch(string DeviceName, DateTime From) : IDeviceCommand;

    public sealed record LoggerBatchResult(ImmutableList<LogBatch> Batches);

    public sealed record QueryDevices;

    public sealed record DevicesResponse(string[] Devices);

    public sealed record QueryUi(string DeviceName) : IDeviceCommand;

    public sealed record UiResponse(DeviceUiGroup Root);
}