using System;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;

namespace Servicemnager.Networking;

public interface IDataServer : IDisposable
{
    event EventHandler<ClientConnectedArgs>? ClientConnected;
    event EventHandler<ClientDisconnectedArgs>? ClientDisconnected;
    event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;
    void Start();
    bool Send(string client, NetworkMessage message);
}