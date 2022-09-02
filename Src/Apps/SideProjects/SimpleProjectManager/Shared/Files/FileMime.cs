using Akkatecture.ValueObjects;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared;

public sealed class FileMime : SingleValueObject<string>
{
    public FileMime(string value) : base(value) { }

    [UsedImplicitly, Obsolete("Used for Serialization")]
    public FileMime()
    {
    }
    
    public static readonly FileMime Generic = new("APPLICATION/octet-stream");
}