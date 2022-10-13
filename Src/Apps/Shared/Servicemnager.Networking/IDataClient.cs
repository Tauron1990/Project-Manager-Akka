using System;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;
using SuperSimpleTcp;

namespace Servicemnager.Networking;

public sealed class ClientConnectedArgs : EventArgs
{
    public ClientConnectedArgs(in Client connection) => Connection = connection;

    public Client Connection { get; }
}

public sealed class ClientDisconnectedArgs : EventArgs
{
    public ClientDisconnectedArgs(in Client connection, DisconnectReason reason)
    {
        Connection = connection;
        Reason = reason;
    }

    /// <summary>
    ///     The IP address and port number of the disconnected client socket.
    /// </summary>
    public Client Connection { get; }

    /// <summary>The reason for the disconnection.</summary>
    public DisconnectReason Reason { get; }
}

public interface IDataClient
{
    bool Connect();
    event EventHandler<ClientConnectedArgs>? Connected;
    event EventHandler<ClientDisconnectedArgs>? Disconnected;
    event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;
    bool Send(NetworkMessage msg);
}