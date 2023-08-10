using System.Runtime.Serialization;
using MemoryPack;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial struct SimpleResultContainer
{
    [IgnoreDataMember, MemoryPackIgnore]
    public SimpleResult SimpleResult { get; }

    [DataMember, MemoryPackOrder(0)]
    public ErrorContainer Error => new(SimpleResult.Error);

    public SimpleResultContainer(SimpleResult simpleResult) =>
        SimpleResult = simpleResult;

    [MemoryPackConstructor]
    public SimpleResultContainer(ErrorContainer error) => 
        SimpleResult = new SimpleResult(error.Error);

    public static implicit operator SimpleResult(SimpleResultContainer container) => container.SimpleResult;
}