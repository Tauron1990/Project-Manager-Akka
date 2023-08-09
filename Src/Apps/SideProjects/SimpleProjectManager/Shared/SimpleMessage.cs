using System.Runtime.Serialization;
using MemoryPack;
using Vogen;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial record struct SimpleMessage([property:DataMember, MemoryPackOrder(0)]string Value) : IStringValueType<SimpleMessage>
{
    public static SimpleMessage GetEmpty { get; } = new(string.Empty);
    public static SimpleMessage From(string value) =>
        new(value);

    public static bool operator ==(SimpleMessage left, string right) =>
        string.Equals(left.Value, right, StringComparison.Ordinal);

    public static bool operator !=(SimpleMessage left, string right) =>
        !string.Equals(left.Value, right, StringComparison.Ordinal);
}