using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record FileList([property:DataMember, MemoryPackOrder(0)]ImmutableList<ProjectFileId> Files);