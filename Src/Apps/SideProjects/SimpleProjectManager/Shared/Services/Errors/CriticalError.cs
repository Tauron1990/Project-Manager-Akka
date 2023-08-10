using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record CriticalError(
    [property:DataMember, MemoryPackOrder(0)]ErrorId Id, 
    [property:DataMember, MemoryPackOrder(1)]DateTime Occurrence, 
    [property:DataMember, MemoryPackOrder(2)]PropertyValue ApplicationPart, 
    [property:DataMember, MemoryPackOrder(3)]SimpleMessage Message, 
    [property:DataMember, MemoryPackOrder(4)]StackTraceData? StackTrace,
    [property:DataMember, MemoryPackOrder(5)]ImmutableList<ErrorProperty> ContextData)
{
    public static readonly CriticalError Empty = 
        new(ErrorId.New, DateTime.MinValue, PropertyValue.Empty, SimpleMessage.Empty, null, ImmutableList<ErrorProperty>.Empty);
}