using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record JobData(
    [property:DataMember, MemoryPackOrder(0)]ProjectId Id, 
    [property:DataMember, MemoryPackOrder(1)]ProjectName JobName, 
    [property:DataMember, MemoryPackOrder(2)]ProjectStatus Status, 
    [property:DataMember, MemoryPackOrder(3)]SortOrder? Ordering, 
    [property:DataMember, MemoryPackOrder(4)]ProjectDeadline? Deadline, 
    [property:DataMember, MemoryPackOrder(5)]ImmutableList<ProjectFileId> ProjectFiles);