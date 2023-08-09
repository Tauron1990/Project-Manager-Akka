using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed record DeviceInputData(
    [property:DataMember, MemoryPackOrder(0)]Ic<DeviceId> Device, 
    [property:DataMember, MemoryPackOrder(1)]Ic<DeviceId> Element, 
    [property:DataMember, MemoryPackOrder(2)]string Data);