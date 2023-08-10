using System.Runtime.Serialization;
using Akkatecture.ValueObjects;
using MemoryPack;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable]
public sealed partial class FileSize : SingleValueObject<long>
{
    public static readonly FileSize Empty = new(0);

    public FileSize(long value) : base(value) { }

    public string ToByteString()
    {
        double first = Value / 1024d;

        return first < 1024 ? $"{first:N2} KB" : $"{first / 1024:N2} MB";
    }
}