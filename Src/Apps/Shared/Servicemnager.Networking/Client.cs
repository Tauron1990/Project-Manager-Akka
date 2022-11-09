using Vogen;

namespace Servicemnager.Networking;

[ValueObject(typeof(string))]
[Instance("All", "All")]
public readonly partial struct Client
{
    public Client PadRight(int toPad)
        => From(Value.PadRight(toPad));
}