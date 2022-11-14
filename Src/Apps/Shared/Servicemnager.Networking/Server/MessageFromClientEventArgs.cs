using System;
using Servicemnager.Networking.Data;

namespace Servicemnager.Networking.Server;

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