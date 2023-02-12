using System.Runtime.InteropServices;
using Vogen;

namespace Tauron.Application.Master.Commands;

[ValueObject(typeof(int))]
[StructLayout(LayoutKind.Auto)]
[Instance("NoVersion", -1)]
#pragma warning disable MA0097
public readonly partial struct SimpleVersion
    #pragma warning restore MA0097
{
    private static Validation Validate(int input)
        => input >= 0 ? Validation.Ok : Validation.Invalid("Version should be Positive");

    public static SimpleVersion operator +(SimpleVersion version, int increment)
        => From(version.Value + increment);
}