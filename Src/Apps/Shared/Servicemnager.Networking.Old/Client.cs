using Vogen;

namespace Servicemnager.Networking;

[ValueObject(typeof(string))]
[Instance("All", "All")]
#pragma warning disable MA0097
public readonly partial struct Client
    #pragma warning restore MA0097
{
    public Client PadRight(int toPad)
        => From(Value.PadRight(toPad));
}