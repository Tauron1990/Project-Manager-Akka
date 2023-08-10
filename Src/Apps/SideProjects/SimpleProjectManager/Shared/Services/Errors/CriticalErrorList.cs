using System.Collections.Immutable;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial struct CriticalErrorList
{
    public static readonly CriticalErrorList Empty = new(ImmutableList<CriticalError>.Empty);

    [DataMember, MemoryPackOrder(0)]
    public ImmutableList<CriticalError> Errors { get; }
    
    public CriticalErrorList(ImmutableList<CriticalError> errors)
        => Errors = errors;
}