using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking;

public interface IDataServer : IDisposable
{
    event EventHandler<ClientConnectedArgs>? ClientConnected;
    event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;
    event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;
    Task Start();
    ValueTask<bool> Send(Client client, NetworkMessage message);
}