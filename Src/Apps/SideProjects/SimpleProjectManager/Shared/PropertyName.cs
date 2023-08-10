using System.Runtime.Serialization;
using MemoryPack;
using Vogen;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial record struct PropertyName([property:DataMember, MemoryPackOrder(0)]string Value) : IStringValueType<PropertyName>
{
    public static PropertyName GetEmpty { get; } = new(string.Empty);
    public static PropertyName From(string value) =>
        new(value);
}