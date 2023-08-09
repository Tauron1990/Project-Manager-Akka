using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record DeviceUiGroup(
    [property:DataMember, MemoryPackOrder(0)]DisplayName Name, 
    [property:DataMember, MemoryPackOrder(1)]UIType Type,
    [property:DataMember, MemoryPackOrder(2)]Ic<DeviceId> Id, 
    [property:DataMember, MemoryPackOrder(3)]ImmutableList<DeviceUiGroup> Ui);