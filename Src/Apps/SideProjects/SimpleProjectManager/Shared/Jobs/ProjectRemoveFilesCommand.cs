using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record ProjectRemoveFilesCommand(
    [property:DataMember, MemoryPackOrder(0)]ProjectId Id, 
    [property:DataMember, MemoryPackOrder(1)]ImmutableList<ProjectFileId> Files);