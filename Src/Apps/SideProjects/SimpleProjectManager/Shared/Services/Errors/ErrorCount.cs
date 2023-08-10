using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[StructLayout(LayoutKind.Auto)]
[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial record struct ErrorCount([property:DataMember, MemoryPackOrder(0)]long Value)
{
    public static bool operator >(ErrorCount left, int right)
        => left.Value > right;

    public static bool operator <(ErrorCount left, int right)
        => left.Value < right;

    public static ErrorCount operator +(ErrorCount left, int right)
        => From(left.Value + right);

    public static ErrorCount From(long value) => new(value);

    public static ErrorCount operator -(ErrorCount left, int right)
        => From(left.Value - right);
}