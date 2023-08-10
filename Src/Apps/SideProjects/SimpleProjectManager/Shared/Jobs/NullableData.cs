using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record NullableData<TType>([property:DataMember, MemoryPackOrder(0)]TType Data);