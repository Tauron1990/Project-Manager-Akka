using Vogen;

namespace Tauron.Application.Master.Commands;

[ValueObject(typeof(int))]
[Instance("NoVersion", -1)]
public readonly partial struct SimpleVersion
{
    private static Validation Validate(int input)
        => input >= 0 ? Validation.Ok : Validation.Invalid("Version should be Positive");
}