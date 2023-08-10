using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record LogBatch(
    [property:DataMember, MemoryPackOrder(0)] DeviceId DeviceName, 
    [property:DataMember, MemoryPackOrder(1)] ImmutableList<LogData> Logs)
{
    [MemoryPackIgnore]
    public bool IsEmpty => Logs.IsEmpty;
}