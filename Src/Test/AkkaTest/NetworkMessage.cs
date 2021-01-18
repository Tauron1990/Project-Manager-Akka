using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Servicemnager.Networking.Data
{
    public sealed class NetworkMessageFormatter
    {
        private readonly MemoryPool<byte> _pool = MemoryPool<byte>.Shared;

        private static readonly byte[] Head = Encoding.ASCII.GetBytes("HEAD");

        private static readonly byte[] End = Encoding.ASCII.GetBytes("ENDING");

        public (IMemoryOwner<byte> Message, int Lenght) WriteMessage(NetworkMessage msg)
        {
            var typeLenght = Encoding.UTF8.GetByteCount(msg.Type);
            var lenght = Head.Length + End.Length + msg.RealLength + typeLenght + 12;

            var message = _pool.Rent(lenght + 4);
            var data = message.Memory.Span;

            Head.CopyTo(data);
            BinaryPrimitives.WriteInt32LittleEndian(data[4..], lenght);

            BinaryPrimitives.WriteInt32LittleEndian(data[8..], typeLenght);
            Encoding.UTF8.GetBytes(msg.Type, data[12..]);
            int pos = 12 + typeLenght;

            var targetLenght = msg.Lenght == -1 ? msg.Data.Length : msg.Lenght;

            BinaryPrimitives.WriteInt32LittleEndian(data[pos..], targetLenght);
            var msgData = targetLenght != msg.Data.Length ? msg.Data.AsMemory()[targetLenght..] : msg.Data.AsMemory();
            var msgPos = message.Memory[(pos + 4)..];

            msgData.CopyTo(msgPos);

            pos += targetLenght + 4;
            End.CopyTo(data[pos..]);

            return (message, lenght);
        }

        public bool HasHeader(byte[] buffer)
        {
            var pos = 0;
            return CheckPresence(buffer, Head, ref pos);
        }

        public bool HasTail(byte[] buffer)
        {
            if (buffer.Length < End.Length)
                return false;

            var pos = buffer.Length - End.Length;
            return CheckPresence(buffer, End, ref pos);
        }

        public NetworkMessage ReadMessage(IMemoryOwner<byte> bufferMemory)
        {
            int bufferPos = 0;
            var buffer = bufferMemory.Memory.Span;

            if (!CheckPresence(buffer, Head, ref bufferPos))
                throw new InvalidOperationException("Invalid Message Format");

            var fullLenght = BinaryPrimitives.ReadInt32LittleEndian(buffer[bufferPos..]);
            bufferPos += 4;

            var typeLenght = BinaryPrimitives.ReadInt32LittleEndian(buffer[bufferPos..]);
            bufferPos += 4;

            var type = Encoding.UTF8.GetString(buffer, bufferPos, typeLenght);
            bufferPos += typeLenght;

            var dataLenght = ReadInt(buffer, ref bufferPos);
            var data = buffer.Skip(bufferPos).Take(dataLenght).ToArray();
            bufferPos += dataLenght;

            if (!CheckPresence(buffer, End, ref bufferPos) || fullLenght != bufferPos)
                throw new InvalidOperationException("Invalid Message Format");

            return new NetworkMessage(type, data, -1);
        }

        public NetworkMessage Create(string type, byte[] data, int lenght = -1) => new(type, data, lenght);

        public NetworkMessage Create(string type) => new(type, Array.Empty<byte>(), -1);

        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckPresence(Span<byte> buffer, IEnumerable<byte> target, ref int pos)
        {
            foreach (var ent in target)
            {
                if (buffer[pos] != ent)
                    return false;

                pos++;
            }

            return true;
        }
    }

    public class NetworkMessage
    {
        public string Type { get; }

        public byte[] Data { get; }

        public int Lenght { get; }

        public int RealLength => Lenght == -1 ? Data.Length : Lenght;

        public NetworkMessage(string type, byte[] data, int lenght)
        {
            Type = type;
            Data = data;
            Lenght = lenght;
        }
    }
}