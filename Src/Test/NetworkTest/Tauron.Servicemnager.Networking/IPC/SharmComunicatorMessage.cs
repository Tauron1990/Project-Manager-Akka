using Vogen;

namespace Tauron.Servicemnager.Networking.IPC;

[Instance("RegisterClient", "RegisterClient")]
[Instance("UnRegisterClient", "UnRegisterClient")]
[ValueObject(typeof(string))]
#pragma warning disable MA0097
public readonly partial struct SharmComunicatorMessage
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => throw new InvalidOperationException("Not Allowed to Create Custom");
}