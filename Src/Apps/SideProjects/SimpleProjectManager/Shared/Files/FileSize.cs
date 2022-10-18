using Akkatecture.ValueObjects;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared;

public sealed class FileSize : SingleValueObject<long>
{
    public FileSize(long value) : base(value) { }
    
    
    public static readonly FileSize Empty = new(0);

    public string ToByteString()
    {
        var first = Value / 1024d;
        
        return first < 1024 ? $"{first:N2} KB" : $"{(first / 1024):N2} MB";
    }
}