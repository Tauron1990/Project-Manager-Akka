namespace Tauron.Servicemnager.Networking;

public sealed class ClientDisconnectedArgs : EventArgs
{
    public ClientDisconnectedArgs(in Client connection, Exception? cause)
    {
        Connection = connection;
        Cause = cause;
    }

    /// <summary>
    ///     The IP address and port number of the disconnected client socket.
    /// </summary>
    public Client Connection { get; }

    public Exception? Cause { get; }
}