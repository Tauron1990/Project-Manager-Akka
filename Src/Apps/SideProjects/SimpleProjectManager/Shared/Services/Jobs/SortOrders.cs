using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record SortOrders([property:DataMember, MemoryPackOrder(0)]ImmutableList<SortOrder> OrdersList);