using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record JobInfo(
    [property:DataMember, MemoryPackOrder(0)]ProjectId Project, 
    [property:DataMember, MemoryPackOrder(1)]ProjectName Name, 
    [property:DataMember, MemoryPackOrder(2)]ProjectDeadline? Deadline, 
    [property:DataMember, MemoryPackOrder(3)]ProjectStatus Status, 
    [property:DataMember, MemoryPackOrder(4)]bool FilesPresent);