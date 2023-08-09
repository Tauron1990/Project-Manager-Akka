using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial record struct FoundDevice(
    [property:DataMember, MemoryPackOrder(0)]DeviceName Name, 
    [property:DataMember, MemoryPackOrder(1)]Ic<DeviceId> Id);