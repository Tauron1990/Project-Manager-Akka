using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking;

public sealed class MessageFromClientEventArgs : EventArgs
{
    public MessageFromClientEventArgs(NetworkMessage message, in Client client)
    {
        Message = message;
        Client = client;
    }

    public NetworkMessage Message { get; }

    public Client Client { get; }
}