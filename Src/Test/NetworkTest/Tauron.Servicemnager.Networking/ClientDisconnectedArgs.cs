using SuperSimpleTcp;

namespace Tauron.Servicemnager.Networking;

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