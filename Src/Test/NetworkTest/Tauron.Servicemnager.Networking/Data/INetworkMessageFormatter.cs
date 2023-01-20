using System.Buffers;

namespace Tauron.Servicemnager.Networking.Data;

public interface INetworkMessageFormatter<out TMessage>
{
    int HeaderLength { get; }
    
    int TailLength { get; }
    
    bool HasHeader(Memory<byte> buffer);
    bool HasTail(Memory<byte> buffer);
    TMessage ReadMessage(Memory<byte> bufferMemory);
}