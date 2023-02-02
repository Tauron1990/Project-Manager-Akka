using System.Collections.Immutable;
using System.Globalization;
using JetBrains.Annotations;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public static class DeviceManagerMessages
{
    public const string DeviceDataId = $"{nameof(DeviceDataId)}--AFFEA925-812A-4B15-8A5F-2DBEF0657E3B";

    public interface IDeviceCommand
    {
        DeviceId DeviceName { get; }
    }

    public sealed record UpdateSensor(DeviceId DeviceName, DeviceId Identifer, ISensorBox SensorValue) : IDeviceCommand;

    public interface ISensorBox : IEquatable<ISensorBox>
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
        public bool Equals(StringSensorBox? other)
        {
            if(ReferenceEquals(null, other))
                return false;
            if(ReferenceEquals(this, other))
                return true;

            return string.Equals(Value, other.Value, StringComparison.Ordinal) && SensorType == other.SensorType;
        }

        public string AsString => Value;

        public int AsInt => int.Parse(Value, NumberStyles.Any, CultureInfo.InvariantCulture);

        public double AsDouble => double.Parse(Value, NumberStyles.Any, CultureInfo.InvariantCulture);

        public bool Equals(ISensorBox? other)
            => other is StringSensorBox box && Equals(box);

        public override int GetHashCode()
            => HashCode.Combine(Value, (int)SensorType);
    }

    [UsedImplicitly]
    public sealed record NumberSensorBox(int Value, SensorType SensorType) : ISensorBox
    {
        public bool Equals(NumberSensorBox? other)
        {
            if(ReferenceEquals(null, other))
                return false;
            if(ReferenceEquals(this, other))
                return true;

            return Value == other.Value && SensorType == other.SensorType;
        }

        public string AsString => Value.ToString(CultureInfo.InvariantCulture);

        public int AsInt => Value;

        public double AsDouble => Value;

        public bool Equals(ISensorBox? other)
            => other is NumberSensorBox box && Equals(box);

        public override int GetHashCode()
            => HashCode.Combine(Value, (int)SensorType);
    }

    [UsedImplicitly]
    public sealed record DoubleSensorBox(double Value, SensorType SensorType) : ISensorBox
    {
        public bool Equals(DoubleSensorBox? other)
        {
            if(ReferenceEquals(null, other))
                return false;
            if(ReferenceEquals(this, other))
                return true;

            return Value.Equals(other.Value) && SensorType == other.SensorType;
        }

        public string AsString => Value.ToString(CultureInfo.InvariantCulture);

        public int AsInt => (int)Value;

        public double AsDouble => Value;

        public bool Equals(ISensorBox? other)
            => other is DoubleSensorBox box && Equals(box);

        public override int GetHashCode()
            => HashCode.Combine(Value, (int)SensorType);
    }

    public sealed record QuerySensorValue(DeviceId DeviceName, DeviceId Identifer) : IDeviceCommand;

    public sealed record SensorValueResult(SimpleResult Result, ISensorBox? Value);

    public sealed record QueryButtonState(DeviceId DeviceName, DeviceId Identifer) : IDeviceCommand;

    public sealed record UpdateButtonState(DeviceId DeviceName, DeviceId Identifer, bool State) : IDeviceCommand;

    public sealed record ButtonClick(DeviceId DeviceName, DeviceId Identifer) : IDeviceCommand;

    public sealed record ButtonStateResponse(bool CanClick);

    public sealed record QueryLoggerBatch(DeviceId DeviceName, DateTime From, DateTime To) : IDeviceCommand;

    public sealed record LoggerBatchResult(ImmutableList<LogBatch> Batches);

    public sealed record QueryDevices;

    public sealed record DevicesResponse(ImmutableDictionary<DeviceId, DeviceName> Devices);

    public sealed record QueryUi(DeviceId DeviceName) : IDeviceCommand;

    public sealed record UiResponse(DeviceUiGroup Root);

    public sealed record NewUIData(DeviceId DeviceName, DeviceUiGroup Ui) : IDeviceCommand;
}