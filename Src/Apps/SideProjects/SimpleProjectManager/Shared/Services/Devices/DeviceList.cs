using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record DeviceList([property:DataMember, MemoryPackOrder(0)]ImmutableArray<FoundDevice> FoundDevices);