using System.Buffers;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Channels;
using Akka.TestKit.Xunit;
using FluentAssertions;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Tests.Device.MGI.Logging;

public sealed class SingleClientManagerTests : TestKit
{
    private static (IMemoryOwner<byte> Data, int Lenght) GetMessages()
    {
        Encoding utf8 = Encoding.UTF8;
        
        Span<byte> msg1 = utf8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "", "TestApp", Command.SetApp)));
        Span<byte> msg2 = utf8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "Type1", "Message1", Command.Log)));
        Span<byte> msg3 = utf8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "Type1", "Message2", Command.Log)));
        Span<byte> msg4 = utf8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "Type2", "Message3", Command.Log)));
        Span<byte> msg5 = utf8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "Type2", "Message4", Command.Log)));
        Span<byte> msg6 = utf8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "", "", Command.Disconnect)));

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
        private readonly IDisposable _disposable;
        
        private ReadOnlySequence<byte> _buffer;
        
        public TestSocket(bool single)
        {
            if(single)
            {
                byte[] array =Encoding.UTF8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "Type1", "Message1", Command.Log)));

                _buffer = new ReadOnlySequence<byte>(array);
                _disposable = Disposable.Empty;
            }
            else
            {
                var messages = GetMessages();

                _buffer = new ReadOnlySequence<byte>(messages.Data.Memory);
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
            
            public TestByteReader(ReadOnlySequence<byte> buffer)
                => _buffer = buffer;

            public void Dispose()
                => _buffer = default;

            public bool HasData => !_sequencePosition.Equals(_buffer.End);

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
            _disposable.Dispose();
            _buffer = default;
        }
    }
    
    [Fact]
    public async Task TestSingleClient()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(10000));
        
        var channel = Channel.CreateUnbounded<LogInfo>();

        ActorOf(() => new SingleClientManager(new TestSocket(true), channel.Writer));

        var result = await channel.Reader.ReadAllAsync(source.Token).ToListAsync(source.Token).ConfigureAwait(false);

        result.Count.Should().Be(1);
    }
}