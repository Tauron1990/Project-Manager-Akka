using System.Buffers;
#pragma warning disable EPS02

namespace Tauron.Servicemnager.Networking.Data;

public interface INetworkMessageFormatter<out TMessage>
    where TMessage : class
{
    Memory<byte> Header { get; }
    
    Memory<byte> Tail { get; }
    
    TMessage ReadMessage(in ReadOnlySequence<byte> bufferMemory);
}