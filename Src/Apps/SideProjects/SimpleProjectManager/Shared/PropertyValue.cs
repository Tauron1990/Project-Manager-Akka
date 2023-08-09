using System.Runtime.Serialization;
using MemoryPack;
using Vogen;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial record struct PropertyValue([property:DataMember, MemoryPackOrder(0)]string Value) : IStringValueType<PropertyValue>
{
    public static PropertyValue GetEmpty { get; } = new(string.Empty);

    public static PropertyValue From(string value) =>
        new(value);
}