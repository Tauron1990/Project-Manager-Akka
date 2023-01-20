using System.Net.Sockets;

namespace SimpleProjectManager.Operation.Client.Device.MGI;

public sealed class RealSocked : ISocket 
{
    private readonly Socket _socket;

    public RealSocked(Socket socket)
        => _socket = socket;

    public void Dispose()
        => _socket.Dispose();

    public bool Poll(int timeout, SelectMode selectMode)
        => _socket.Poll(timeout, selectMode);

    public long Available => _socket.Available;
    public ValueTask<int> ReceiveAsync(Memory<byte> buffer)
        => _socket.ReceiveAsync(buffer);

    public void Close()
        => _socket.Close();
}