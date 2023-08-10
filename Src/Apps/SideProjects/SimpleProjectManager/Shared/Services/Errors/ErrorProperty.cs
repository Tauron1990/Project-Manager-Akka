using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record ErrorProperty(
    [property:DataMember, MemoryPackOrder(0)]PropertyName Key,
    [property:DataMember, MemoryPackOrder(1)]PropertyValue Value);