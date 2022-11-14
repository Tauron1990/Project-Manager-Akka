using Vogen;

namespace Servicemnager.Networking;

[Instance("Identifer", "Identifer")]
[Instance("Deny", "Deny")]
[Instance("Accept", "Accept")]
[Instance("Data", "Data")]
[Instance("Compled", "Compled")]
[Instance("Message", "Message")]
[Instance("DataAccept", "DataAccept")]
[Instance("DataNext", "DataNext")]
[Instance("DataChunk", "DataChunk")]
[Instance("DataCompled", "DataCompled")]
[ValueObject(typeof(string))]
#pragma warning disable MA0097
public readonly partial struct NetworkOperation
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => Validation.Invalid("No Individual Value Allowed");
}