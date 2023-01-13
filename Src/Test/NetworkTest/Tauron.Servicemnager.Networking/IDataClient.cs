using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking;

public interface IDataClient
{
    bool Connect();
    event EventHandler<ClientConnectedArgs>? Connected;
    event EventHandler<ClientDisconnectedArgs>? Disconnected;
    event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;
    bool Send(NetworkMessage msg);
}