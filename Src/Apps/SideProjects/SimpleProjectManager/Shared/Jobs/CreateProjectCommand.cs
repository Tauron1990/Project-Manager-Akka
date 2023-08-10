using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record CreateProjectCommand(
    [property:DataMember, MemoryPackOrder(0)]ProjectName Project, 
    [property:DataMember, MemoryPackOrder(1)]ImmutableList<ProjectFileId> Files, 
    [property:DataMember, MemoryPackOrder(2)]ProjectStatus Status,
    [property:DataMember, MemoryPackOrder(3)]ProjectDeadline? Deadline);