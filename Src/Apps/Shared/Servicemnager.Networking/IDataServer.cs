using System;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;
using SimpleTcp;

namespace Servicemnager.Networking
{
    public interface IDataServer : IDisposable
    {
        event EventHandler<ClientConnectedEventArgs> ClientConnected;
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        event EventHandler<MessageFromClientEventArgs>? OnMessageReceived;
        void Start();
        void Send(string client, NetworkMessage message);
    }
}