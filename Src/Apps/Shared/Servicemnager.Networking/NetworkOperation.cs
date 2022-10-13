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
public readonly partial struct NetworkOperation
{
    private static Validation Validate(string value)
        => Validation.Invalid("No Individual Value Allowed");
}