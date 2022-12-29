using Vogen;

namespace SimpleProjectManager.Operation.Client.Config;

[ValueObject(typeof(int))]
[Instance("Empty", -1)]
public readonly partial struct Port
{
    private static Validation Validate(int port)
        => port switch
        {
            < 0 => Validation.Invalid("The Port Number ist to Low"),
            > 65_535 => Validation.Invalid("The Port Number is to High"),
            _ => Validation.Ok,
        };
}