using System;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;
using SimpleTcp;

namespace Servicemnager.Networking
{
    public sealed class ClientConnectedArgs : EventArgs
    {
        public ClientConnectedArgs(string connection) => Connection = connection;
        
        public string Connection { get; }
    }

    public sealed class ClientDisconnectedArgs : EventArgs
    {
        public ClientDisconnectedArgs(string connection, DisconnectReason reason)
        {
            Connection = connection;
            Reason = reason;
        }

        /// <summary>
        /// The IP address and port number of the disconnected client socket.
        /// </summary>
        public string Connection { get; }

        /// <summary>The reason for the disconnection.</summary>
        public DisconnectReason Reason { get; }
    }

    public interface IDataClient
    {
        void Connect();
        event EventHandler<ClientConnectedArgs>? Connected;
        event EventHandler<ClientDisconnectedArgs>? Disconnected;
        event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;
        void Send(NetworkMessage msg);
    }
}