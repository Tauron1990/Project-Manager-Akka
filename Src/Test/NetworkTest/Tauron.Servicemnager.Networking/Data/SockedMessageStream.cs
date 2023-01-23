using System.Net.Sockets;

namespace Tauron.Servicemnager.Networking.Data;

public sealed class SocketMessageStream : IMessageStream
{
    private readonly Socket _socket;
    
    private readonly NetworkStream _networkStream;

    public bool DataAvailable => _socket.Available != 0;
    
    public Stream ReadStream { get; }

    public SocketMessageStream(Socket socket)
    {
        _socket = socket;
        _networkStream = new NetworkStream(socket);
    }

    public bool Connected()
        => SocketConnected(_socket);

    private static bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        return !part1 || !part2;

    }

    public void Dispose()
    {
        _socket.Dispose();
        _networkStream.Dispose();
    }
}