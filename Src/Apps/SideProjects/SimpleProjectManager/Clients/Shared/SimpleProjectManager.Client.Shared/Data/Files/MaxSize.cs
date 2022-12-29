using Vogen;

namespace SimpleProjectManager.Client.Shared.Data.Files;

[ValueObject(typeof(long))]
[Instance("Max", long.MaxValue)]
[Instance("MaxFileSize", 1_073_741_824L)]
public readonly partial struct MaxSize
{
    private static Validation Validate(long value)
        => value < 0 ? Validation.Invalid("Size must be Positive") : Validation.Ok;

    public static bool operator <(long lenght, MaxSize maxSize)
        => lenght < maxSize.Value;

    public static bool operator >(long lenght, MaxSize maxSize)
        => lenght > maxSize.Value;
}