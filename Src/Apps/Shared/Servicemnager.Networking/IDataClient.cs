using System;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;
using SimpleTcp;

namespace Servicemnager.Networking
{
    public interface IDataClient
    {
        void Connect();
        event EventHandler<ClientConnectedEventArgs>? Connected;
        event EventHandler<ClientDisconnectedEventArgs>? Disconnected;
        event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;
        void Send(NetworkMessage msg);
    }
}