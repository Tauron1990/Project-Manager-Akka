using Vogen;

namespace SimpleProjectManager.Shared.Services;

[ValueObject(typeof(long))]
public readonly partial struct ErrorCount
{
    private static Validation Validate(long value)
        => value < 0 ? Validation.Invalid("Error Count must be Greater then zero") : Validation.Ok;
    
    public static bool operator >(ErrorCount left, int right)
        => left.Value > right;
    
    public static bool operator <(ErrorCount left, int right)
        => left.Value < right;
    
    public static ErrorCount operator +(ErrorCount left, int right)
        => From(left.Value + right);
    
    public static ErrorCount operator -(ErrorCount left, int right)
        => From(left.Value - right);
}