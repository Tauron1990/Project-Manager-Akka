using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record Jobs([property:DataMember, MemoryPackOrder(0)]ImmutableList<JobInfo> JobInfos);