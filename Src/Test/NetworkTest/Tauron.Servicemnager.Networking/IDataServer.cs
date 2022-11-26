using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking;

public interface IDataServer : IDisposable
{
    event EventHandler<ClientConnectedArgs>? ClientConnected;
    event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;
    event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;
    void Start();
    bool Send(in Client client, NetworkMessage message);
}