using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using MemoryPack;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Shared.Services.Devices;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed record LogData(
    [property:DataMember, MemoryPackOrder(0)] LogLevel LogLevel, 
    [property:DataMember, MemoryPackOrder(1)] LogCategory Category, 
    [property:DataMember, MemoryPackOrder(2)] SimpleMessage Message, 
    [property:DataMember, MemoryPackOrder(3)] DateTime Occurance, 
    [property:DataMember, MemoryPackOrder(4)] ImmutableDictionary<string, PropertyValue> Data);