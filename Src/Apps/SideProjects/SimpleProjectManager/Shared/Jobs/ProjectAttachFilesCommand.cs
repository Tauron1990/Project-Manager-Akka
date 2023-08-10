using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record ProjectAttachFilesCommand(
    [property:DataMember, MemoryPackOrder(0)]ProjectId Id, 
    [property:DataMember, MemoryPackOrder(1)]ProjectName Name, 
    [property:DataMember, MemoryPackOrder(2)]ImmutableList<ProjectFileId> Files);