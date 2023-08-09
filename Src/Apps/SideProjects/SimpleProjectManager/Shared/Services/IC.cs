using System.Runtime.Serialization;
using Akkatecture.Core;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class Ic<TId>
    where TId : Identity<TId>
{
    private readonly TId _identity;

    [DataMember, MemoryPackOrder(0)]
    public string Value => _identity.Value;

    public Ic(TId id) => _identity = id;

    [MemoryPackConstructor]
    public Ic(string value) => _identity = Identity<TId>.With(value);

    public static implicit operator TId(Ic<TId> container)
        => container._identity;
    
    public static implicit operator Ic<TId>(TId id)
        => new(id);
}