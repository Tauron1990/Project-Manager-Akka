using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record Logs([property:DataMember, MemoryPackOrder(0)] ImmutableList<LogBatch> Data);