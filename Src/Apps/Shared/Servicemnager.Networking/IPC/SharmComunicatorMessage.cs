using System;
using Vogen;

namespace Servicemnager.Networking.IPC;

[Instance("RegisterClient", "RegisterClient")]
[Instance("UnRegisterClient", "UnRegisterClient")]
[ValueObject(typeof(string))]
public readonly partial struct SharmComunicatorMessage
{
    
    
    private static Validation Validate(string value)
        => throw new InvalidOperationException("Not Allowed to Create Custom");
}