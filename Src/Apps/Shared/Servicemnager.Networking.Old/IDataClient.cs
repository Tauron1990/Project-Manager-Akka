using System;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;

namespace Servicemnager.Networking;

public interface IDataClient
{
    bool Connect();
    event EventHandler<ClientConnectedArgs>? Connected;
    event EventHandler<ClientDisconnectedArgs>? Disconnected;
    event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;
    bool Send(NetworkMessage msg);
}