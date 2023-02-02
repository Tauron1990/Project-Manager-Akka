using System.Net.Sockets;

namespace Tauron.Servicemnager.Networking.Data;

public sealed class SocketMessageStream : IMessageStream
{
    private readonly Socket _socket;
    
    private readonly IByteReader _networkStream;

    public bool DataAvailable => _socket.Available != 0;

    public IByteReader ReadStream => _networkStream;

    public SocketMessageStream(Socket socket)
    {
        _socket = socket;
        _networkStream = IByteReader.Stream(new NetworkStream(socket));
    }

    public bool Connected()
        => _socket.IsConnected();

    public void Dispose()
    {
        _socket.Dispose();
        _networkStream.Dispose();
    }
}