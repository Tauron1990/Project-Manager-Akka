using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record DeviceInputData(
    [property:DataMember, MemoryPackOrder(0)]DeviceId Device, 
    [property:DataMember, MemoryPackOrder(1)]DeviceId Element, 
    [property:DataMember, MemoryPackOrder(2)]string Data);