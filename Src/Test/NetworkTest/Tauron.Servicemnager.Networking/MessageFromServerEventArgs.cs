using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking;

public class MessageFromServerEventArgs : EventArgs
{
    public MessageFromServerEventArgs(NetworkMessage message) => Message = message;
    public NetworkMessage Message { get; }
}