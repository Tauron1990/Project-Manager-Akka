using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public record LogData(
    [property:DataMember, MemoryPackOrder(0)]string Date, 
    [property:DataMember, MemoryPackOrder(0)]string Level, 
    [property:DataMember, MemoryPackOrder(0)]string EventType, 
    [property:DataMember, MemoryPackOrder(0)]string Message, 
    [property:DataMember, MemoryPackOrder(0)]ImmutableDictionary<string, string> Propertys);