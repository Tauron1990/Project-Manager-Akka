using System.Runtime.Serialization;
using MemoryPack;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial struct ErrorContainer
{
    [IgnoreDataMember, MemoryPackIgnore]
    public Error? Error { get; set; }

    [DataMember, MemoryPackOrder(0)]
    public string? Code => Error?.Code;

    [DataMember, MemoryPackOrder(1)]
    public string? Info => Error?.Info;

    public ErrorContainer(Error? error) => Error = error;

    [MemoryPackConstructor]
    public ErrorContainer(string code, string? info) => Error = new Error(info, code);
}