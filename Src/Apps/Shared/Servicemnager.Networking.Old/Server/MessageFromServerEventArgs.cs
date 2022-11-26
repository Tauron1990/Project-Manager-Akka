using System;
using Servicemnager.Networking.Data;

namespace Servicemnager.Networking.Server;

public class MessageFromServerEventArgs : EventArgs
{
    public MessageFromServerEventArgs(NetworkMessage message) => Message = message;
    public NetworkMessage Message { get; }
}