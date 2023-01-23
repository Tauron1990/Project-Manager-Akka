using System.Net.Sockets;

namespace SimpleProjectManager.Operation.Client.Device.MGI;

public interface ISocket : IDisposable
{
    bool Poll(int timeout, SelectMode selectMode);
    long Available { get; }
    ValueTask<int> ReceiveAsync(Memory<byte> buffer);
    void Close();
}