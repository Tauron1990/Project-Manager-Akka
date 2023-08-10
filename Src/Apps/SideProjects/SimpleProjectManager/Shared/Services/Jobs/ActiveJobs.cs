using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using MemoryPack;
using Vogen;

namespace SimpleProjectManager.Shared.Services;

[StructLayout(LayoutKind.Auto)]
[MemoryPackable(GenerateType.VersionTolerant), DataContract]
public readonly partial record struct ActiveJobs([property:DataMember, MemoryPackOrder(0)]int Value)
{
    public static ActiveJobs From(int value) => new(value);
}