using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleProjectManager.Operation.Client.Device.MGI;

public interface ISocket : IDisposable
{
    bool Poll(int timeout, SelectMode selectMode);
    long Available { get; }
    ValueTask<int> ReceiveAsync(Memory<byte> buffer);
    void Close();
}