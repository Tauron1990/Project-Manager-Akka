using System.Runtime.InteropServices;
using Vogen;

namespace SimpleProjectManager.Shared.Services;

[ValueObject(typeof(long))]
[StructLayout(LayoutKind.Auto)]
#pragma warning disable MA0097
public readonly partial struct ActiveJobs
    #pragma warning restore MA0097
{
    private static Validation Validate(long value)
        => value >= 0 ? Validation.Ok : Validation.Invalid("Cound Should be Posotive");
}