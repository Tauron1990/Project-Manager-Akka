using System.Collections.Immutable;
using JetBrains.Annotations;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public static class DeviceManagerMessages
{
    public sealed record DeviceInfoResponse(bool Duplicate, SimpleResult Result);

    public interface IDeviceCommand
    {
        DeviceId DeviceName { get; }
    }

    public sealed record UpdateSensor(DeviceId DeviceName, DeviceId Identifer, ISensorBox SensorValue) : IDeviceCommand;

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
                SensorType.String => Create("Empty"),
                SensorType.Number => Create(0),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid Sensor Type"),
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

    public sealed record QuerySensorValue(DeviceId DeviceName, DeviceId Identifer) : IDeviceCommand;

    public sealed record SensorValueResult(SimpleResult Result, ISensorBox? Value);

    public sealed record QueryButtonState(DeviceId DeviceName, DeviceId Identifer) : IDeviceCommand;

    public sealed record UpdateButtonState(DeviceId DeviceName, DeviceId Identifer, bool State) : IDeviceCommand;

    public sealed record ButtonClick(DeviceId DeviceName, DeviceId Identifer) : IDeviceCommand;

    public sealed record ButtonStateResponse(bool CanClick);

    public sealed record QueryLoggerBatch(DeviceId DeviceName, DateTime From) : IDeviceCommand;

    public sealed record LoggerBatchResult(ImmutableList<LogBatch> Batches);

    public sealed record QueryDevices;

    public sealed record DevicesResponse(ImmutableDictionary<DeviceId, DeviceName> Devices);

    public sealed record QueryUi(DeviceId DeviceName) : IDeviceCommand;

    public sealed record UiResponse(DeviceUiGroup Root);
}