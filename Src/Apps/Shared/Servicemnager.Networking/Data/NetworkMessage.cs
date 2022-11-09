using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Servicemnager.Networking.Data;

public sealed class NetworkMessageFormatter
{
    private static readonly byte[] Head = Encoding.ASCII.GetBytes("HEAD");

    public static readonly byte[] End = Encoding.ASCII.GetBytes("ENDING");

    public static readonly NetworkMessageFormatter Shared = new(MemoryPool<byte>.Shared);

    private readonly MemoryPool<byte> _pool;

    public NetworkMessageFormatter(MemoryPool<byte> pool) => _pool = pool;

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

    public bool HasHeader(Memory<byte> buffer)
    {
        var pos = 0;

        return CheckPresence(buffer.Span, Head, ref pos);
    }

    public bool HasTail(Memory<byte> buffer)
    {
        if(buffer.Length < End.Length)
            return false;

        int pos = buffer.Length - End.Length;

        return CheckPresence(buffer.Span, End, ref pos);
    }

    public NetworkMessage ReadMessage(Memory<byte> bufferMemory)
    {
        var bufferPos = 0;
        var buffer = bufferMemory.Span;

        if(!CheckPresence(buffer, Head, ref bufferPos))
            throw new InvalidOperationException("Invalid Message Format");

        int fullLenght = BinaryPrimitives.ReadInt32LittleEndian(buffer[bufferPos..]);
        bufferPos += 4;

        int typeLenght = BinaryPrimitives.ReadInt32LittleEndian(buffer[bufferPos..]);
        bufferPos += 4;

        string type = Encoding.UTF8.GetString(
            buffer[bufferPos..(bufferPos + typeLenght)]); //Encoding.UTF8.GetString(buffer, bufferPos, typeLenght);
        bufferPos += typeLenght;

        int dataLenght = BinaryPrimitives.ReadInt32LittleEndian(buffer[bufferPos..]);
        bufferPos += 4;

        byte[] data = buffer[bufferPos..(bufferPos + dataLenght)].ToArray();
        bufferPos += dataLenght;

        if(!CheckPresence(buffer, End, ref bufferPos) || fullLenght != bufferPos)
            throw new InvalidOperationException("Invalid Message Format");

        return new NetworkMessage(type, data, -1);
    }

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckPresence(Span<byte> buffer, IEnumerable<byte> target, ref int pos)
    {
        foreach (byte ent in target)
        {
            if(buffer[pos] != ent)
                return false;

            pos++;
        }

        return true;
    }
}

public record NetworkMessage(string Type, byte[] Data, int Lenght)
{
    public int RealLength => Lenght == -1 ? Data.Length : Lenght;

    public static NetworkMessage Create(string type, byte[] data, int lenght = -1) => new(type, data, lenght);

    public static NetworkMessage Create(string type) => new(type, Array.Empty<byte>(), -1);
}