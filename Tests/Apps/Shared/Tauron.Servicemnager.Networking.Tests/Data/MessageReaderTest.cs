using System.Text;
using System.Threading.Channels;
using FluentAssertions;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.Tests.Data;

public sealed class MessageReaderTest
{
    private static readonly NetworkMessage[] TestMessages =
    {
        NetworkMessage.Create("Test", "Test"u8.ToArray()),
        NetworkMessage.Create("Test1", "Test1"u8.ToArray()),
        NetworkMessage.Create("Test2", "Test2"u8.ToArray()),
        NetworkMessage.Create("Test3", "Test3"u8.ToArray()),
        NetworkMessage.Create("Test4", "Test4"u8.ToArray()),
        NetworkMessage.Create("Test5", "Test5"u8.ToArray()),
    };

    
    private sealed class MessageTestSource : IMessageStream
    {
        private readonly MemoryStream _messages = new();

        internal MessageTestSource(IEnumerable<NetworkMessage> messages)
        {
            foreach (NetworkMessage message in messages)
            {
                var result = NetworkMessageFormatter.Shared.WriteMessage(message);
                using var data = result.Message;
                _messages.Write(data.Memory[..result.Lenght].Span);
            }

            _messages.Position = 0;

            ReadStream = IByteReader.Stream(_messages);
        }

        public void Dispose()
            => _messages.Dispose();

        public bool DataAvailable => _messages.Position != _messages.Length;
        public IByteReader ReadStream { get; }
        public bool Connected()
            => DataAvailable;
    }
    
    [Fact]
    public async Task MessageReaderSingleMessageTest()
    {
        using var data = new MessageTestSource(TestMessages.Take(1));
        using var reader = new MessageReader<NetworkMessage>(data, NetworkMessageFormatter.Shared);
        var channel = Channel.CreateUnbounded<NetworkMessage>();

        await reader.ReadAsync(channel.Writer, default);

        var result = await channel.Reader.ReadAllAsync().ToListAsync();

        result.Should().HaveCount(1);
        NetworkMessage msg = result[0];
        msg.Type.Should().Be("Test");
        Encoding.UTF8.GetString(msg.Data).Should().Be("Test");
    }

    [Fact]
    public async Task MessageReaderMultiMessageTest()
    {
        using var data = new MessageTestSource(TestMessages);
        using var reader = new MessageReader<NetworkMessage>(data, NetworkMessageFormatter.Shared);
        var channel = Channel.CreateUnbounded<NetworkMessage>();

        await reader.ReadAsync(channel.Writer, default);

        var result = await channel.Reader.ReadAllAsync().ToListAsync();

        result.Should().HaveCount(TestMessages.Length);

        foreach (NetworkMessage msg in result)
        {
            msg.Type.Should().StartWith("Test");
            Encoding.UTF8.GetString(msg.Data).Should().StartWith("Test");
        }
    }
}