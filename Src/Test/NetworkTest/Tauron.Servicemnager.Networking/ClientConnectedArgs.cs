namespace Tauron.Servicemnager.Networking;

public sealed class ClientConnectedArgs : EventArgs
{
    public ClientConnectedArgs(in Client connection) => Connection = connection;

    public Client Connection { get; }
}