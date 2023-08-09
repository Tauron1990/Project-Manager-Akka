using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial record struct DisplayName([property:DataMember, MemoryPackOrder(0)]string Value)
{
    public static DisplayName From(string name) => new(name);
}