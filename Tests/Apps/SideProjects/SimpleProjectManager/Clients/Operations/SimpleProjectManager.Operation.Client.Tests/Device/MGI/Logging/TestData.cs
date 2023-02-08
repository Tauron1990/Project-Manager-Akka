using System.Buffers;
using System.Text;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Tests.Device.MGI.Logging;

public class TestData
{
    public static IMessageStream Single()
        => new TestSocket(single: true);

    public static IMessageStream Multi()
        => new TestSocket(single: false);

    private static (IMemoryOwner<byte> Data, int Lenght) GetMessages()
    {
        Encoding utf8 = Encoding.UTF8;

        Span<byte> msg1 = utf8.GetBytes(LoggerTcpFormat.GetMessage(new LogInfo(DateTime.Now, "", "", "TestApp", Command.SetApp)));
        Span<byte> msg2 = utf8.GetBytes(LoggerTcpFormat.GetMessage(new LogInfo(DateTime.Now, "", "Type1", "Message1", Command.Log)));
        Span<byte> msg3 = utf8.GetBytes(LoggerTcpFormat.GetMessage(new LogInfo(DateTime.Now, "", "Type1", "Message2", Command.Log)));
        Span<byte> msg4 = utf8.GetBytes(LoggerTcpFormat.GetMessage(new LogInfo(DateTime.Now, "", "Type2", "Message3", Command.Log)));
        Span<byte> msg5 = utf8.GetBytes(LoggerTcpFormat.GetMessage(new LogInfo(DateTime.Now, "", "Type2", "Message4", Command.Log)));
        Span<byte> msg6 = utf8.GetBytes(LoggerTcpFormat.GetMessage(new LogInfo(DateTime.Now, "", "", "", Command.Disconnect)));

        int lenght = msg1.Length + msg2.Length + msg3.Length + msg4.Length + msg5.Length + msg6.Length;
        var data = MemoryPool<byte>.Shared.Rent(lenght);

        var dataContent = data.Memory.Span;

        msg1.CopyTo(dataContent);
        msg2.CopyTo(dataContent[(msg1.Length)..]);
        msg3.CopyTo(dataContent[(msg1.Length + msg2.Length)..]);
        msg4.CopyTo(dataContent[(msg1.Length + msg2.Length + msg3.Length)..]);
        msg5.CopyTo(dataContent[(msg1.Length + msg2.Length + msg3.Length + msg4.Length)..]);
        msg6.CopyTo(dataContent[(msg1.Length + msg2.Length + msg3.Length + msg4.Length + msg5.Length)..]);

        return (data, lenght);
    }

    private sealed class TestSocket : IMessageStream
    {
        private readonly TestByteReader _byteReader;
        private readonly IDisposable? _disposable;
        
        private ReadOnlySequence<byte> _buffer;
        
        internal TestSocket(bool single)
        {
            if(single)
            {
                byte[] array =Encoding.UTF8.GetBytes(LoggerTcpFormat
                    .GetMessage(new LogInfo(DateTime.Now, "", "Type1", "Message1", Command.Log)));

                _buffer = new ReadOnlySequence<byte>(array);
            }
            else
            {
                var messages = GetMessages();

                _buffer = new ReadOnlySequence<byte>(messages.Data.Memory[..messages.Lenght]);
                _disposable = messages.Data;
            }

            _byteReader = new TestByteReader(_buffer);
        }

        public bool DataAvailable => _byteReader.HasData;

        public IByteReader ReadStream => _byteReader;
        
        public bool Connected()
            => _byteReader.HasData;

        private sealed class TestByteReader : IByteReader
        {
            private readonly Random _random = new();
            
            private ReadOnlySequence<byte> _buffer;
            private SequencePosition _sequencePosition;
            
            internal TestByteReader(ReadOnlySequence<byte> buffer)
                => _buffer = buffer;

            public void Dispose()
                => _buffer = default;

            internal bool HasData => !_sequencePosition.Equals(_buffer.End);

            public ValueTask<int> ReadAsync(Memory<byte> mem, CancellationToken token)
            {
                int toRead = _random.Next(1, (int)Math.Min(10, _buffer.Length - _sequencePosition.GetInteger()));

                var reader = new SequenceReader<byte>(_buffer);
                reader.Advance(_sequencePosition.GetInteger());

                if(!reader.TryReadExact(toRead, out var data))
                    return ValueTask.FromResult(0);

                data.CopyTo(mem.Span);
                _sequencePosition = reader.Position;

                return ValueTask.FromResult(toRead);
            }
        }

        public void Dispose()
        {
            _byteReader.Dispose();
            _disposable?.Dispose();
            _buffer = default;
        }
    }
}