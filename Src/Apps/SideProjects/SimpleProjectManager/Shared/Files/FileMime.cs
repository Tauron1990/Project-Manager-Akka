using System.Runtime.Serialization;
using Akkatecture.ValueObjects;
using MemoryPack;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable]
public sealed partial class FileMime : SingleValueObject<string>
{
    public static readonly FileMime Generic = new("APPLICATION/octet-stream");

    public FileMime(string value) : base(value) { }
}