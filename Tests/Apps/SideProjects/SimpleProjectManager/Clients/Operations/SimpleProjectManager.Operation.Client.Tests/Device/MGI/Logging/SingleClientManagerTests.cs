using System.Buffers;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Channels;
using Akka.TestKit.Xunit;
using FluentAssertions;
using SimpleProjectManager.Operation.Client.Device.MGI;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

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

    private sealed class TestSocket : ISocket
    {
        private readonly int _lenght;
        private readonly Memory<byte> _data;
        private readonly Random _random = new();
        private readonly IDisposable _disposable;
        
        private int _index;
        public TestSocket(bool single)
        {
            if(single)
            {
                byte[] array =Encoding.UTF8.GetBytes(LogInfo.Format(new LogInfo(DateTime.Now, "", "Type1", "Message1", Command.Log)));

                _lenght = array.Length;
                _data = array;
                _disposable = Disposable.Empty;
            }
            else
            {
                var messages = GetMessages();

                _lenght = messages.Lenght;
                _index = 0;
                _data = messages.Data.Memory;
                _disposable = messages.Data;
            }
        }

        public void Dispose()
            => _disposable.Dispose();

        public bool Poll(int timeout, SelectMode selectMode)
            => _index < _lenght;

        public long Available => _lenght - _index;

        public ValueTask<int> ReceiveAsync(Memory<byte> buffer)
        {
            int toRead = _random.Next(0, Math.Min(20, (int)Available));
            
            if(toRead == 0)
                return ValueTask.FromResult(0);
            
            _data.Span[_index..(_index + toRead)].CopyTo(buffer.Span);

            _index += toRead;
            return ValueTask.FromResult(toRead);
        }

        public void Close()
            => _index = _lenght;
    }
    
    [Fact]
    public async Task TestClient()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(10000));
        
        var channel = Channel.CreateUnbounded<LogInfo>();

        ActorOf(() => new SingleClientManager(new TestSocket(true), channel.Writer));

        var result = await channel.Reader.ReadAllAsync(source.Token).ToListAsync(source.Token).ConfigureAwait(false);

        result.Count.Should().Be(6);
    }
}