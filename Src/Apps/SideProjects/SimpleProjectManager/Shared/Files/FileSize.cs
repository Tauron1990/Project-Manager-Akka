using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class FileSize : SingleValueObject<long>
{
    public FileSize(long value) : base(value) { }

    public static readonly FileSize Empty = new(0);

    public string ToMegaByteString()
        => $"{Value / 1024 / 1024} Mb";
}