using Vogen;

namespace Tauron.Servicemnager.Networking.IPC;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct SharmProcessId
{
    public bool IsEmpty => String.IsNullOrWhiteSpace(Value);
}
#pragma warning restore MA0097