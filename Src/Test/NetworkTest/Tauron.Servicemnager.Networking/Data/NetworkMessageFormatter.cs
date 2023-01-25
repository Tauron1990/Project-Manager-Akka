using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Tauron.Servicemnager.Networking.Data;

public sealed class NetworkMessageFormatter : INetworkMessageFormatter<NetworkMessage>
{
    public static readonly NetworkMessageFormatter Shared = new(MemoryPool<byte>.Shared);

    private readonly MemoryPool<byte> _pool;

    private NetworkMessageFormatter(MemoryPool<byte> pool) => _pool = pool;

    public (IMemoryOwner<byte> Message, int Lenght) WriteMessage(NetworkMessage msg, Func<int, (IMemoryOwner<byte> Memory, int Start)>? allocate = null)
    {
        int typeLenght = Encoding.UTF8.GetByteCount(msg.Type);
        int lenght = Head.Length + End.Length + msg.RealLength + typeLenght + 12;
        var msgStart = 0;

        IMemoryOwner<byte>? memoryOwner = null;

        try
        {
            Memory<byte> message;
            Span<byte> data;

            if(allocate is null)
            {
                memoryOwner = _pool.Rent(lenght + 4);
                message = memoryOwner.Memory;
                data = memoryOwner.Memory.Span;
            }
            else
            {
                (var owner, int start) = allocate(lenght + 4);
                msgStart = start;
                memoryOwner = owner;
                message = start == 0 ? memoryOwner.Memory : memoryOwner.Memory[start..];
                data = message.Span;
            }

            Head.CopyTo(data);
            BinaryPrimitives.WriteInt32LittleEndian(data[4..], lenght);

            BinaryPrimitives.WriteInt32LittleEndian(data[8..], typeLenght);
            Encoding.UTF8.GetBytes(msg.Type, data[12..]);
            int pos = 12 + typeLenght;

            int targetLenght = msg.Lenght == -1 ? msg.Data.Length : msg.Lenght;

            BinaryPrimitives.WriteInt32LittleEndian(data[pos..], targetLenght);
            var msgData = targetLenght != msg.Data.Length
                ? msg.Data.AsMemory()[targetLenght..]
                : msg.Data.AsMemory();
            var msgPos = message[(pos + 4)..];

            msgData.CopyTo(msgPos);

            pos += targetLenght + 4;
            End.CopyTo(data[pos..]);

            return (memoryOwner, lenght + msgStart);
        }
        catch
        {
            memoryOwner?.Dispose();

            throw;
        }
    }

    public int HeaderLength => Head.Length;
    public int TailLength => End.Length;
    

    private static readonly byte[] Head = "HEAD"u8.ToArray();

    private static readonly byte[] End = "ENDING"u8.ToArray();

    public Memory<byte> Header => Head;
    public Memory<byte> Tail => End;

    public NetworkMessage ReadMessage(in ReadOnlySequence<byte> bufferMemory)
    {
        var buffer = new SequenceReader<byte>(bufferMemory);

        if(!buffer.TryReadLittleEndian(out int _))
            throw InvalidFormat("Lenght");
        if(!buffer.TryReadLittleEndian(out int typeLenght))
            throw InvalidFormat("Type Lenght");
        if(!buffer.TryReadExact(typeLenght, out var typeData))
            throw InvalidFormat("Type Data");

        string type = Encoding.UTF8.GetString(typeData);

        if(!buffer.TryReadLittleEndian(out int dataLenght))
            throw InvalidFormat("Data Lenght");

        if(buffer.TryReadExact(dataLenght, out var data))
        {
            return new NetworkMessage(type, data.ToArray(), -1);
        }
        throw InvalidFormat("Data");
    }

    private static Exception InvalidFormat(string type)
        => new InvalidOperationException($"Invalid Message Format: {type}");
}