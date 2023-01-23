using System.Buffers;

namespace Tauron.Servicemnager.Networking.Data;

public interface INetworkMessageFormatter<out TMessage>
{
    byte[] Header { get; }
    
    byte[] Tail { get; }
    
    TMessage ReadMessage(Memory<byte> bufferMemory);
}