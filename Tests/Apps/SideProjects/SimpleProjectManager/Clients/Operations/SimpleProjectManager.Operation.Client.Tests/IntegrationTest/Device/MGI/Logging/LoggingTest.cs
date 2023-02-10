using System.Threading.Channels;
using Akka.Actor;
using Akka.TestKit.Xunit;
using FluentAssertions;
using SimpleProjectManager.Operation.Client.Device.MGI;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

namespace SimpleProjectManager.Operation.Client.Tests.IntegrationTest.Device.MGI.Logging;

public sealed class LoggingTest : TestKit
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TestLogger(bool single)
    {
        using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var generator = new LogsGenerator(single ? 1 : 2, single ? 1 : 1000, cancel.Token);
        var channel = Channel.CreateUnbounded<LogInfo>();
        var list = new List<LogInfo>();

        IActorRef? actor = ActorOf(() => new LoggerServer(channel.Writer, MgiMachine.Port));

        cancel.Token.Register(() =>
        {
            channel.Writer.TryComplete();
            actor.Tell(PoisonPill.Instance);
        });
        await Task.WhenAll(generator.DoWrite(), DoRead(channel.Reader, list, cancel.Token));

        list.Should().AllSatisfy(i => i.Type.Should().NotBe("Crash"));
        list.Should().HaveCount(single ? 3 : 2002);

        async Task DoRead(ChannelReader<LogInfo> reader, ICollection<LogInfo> messages, CancellationToken token)
        {
            await foreach (LogInfo msg in reader.ReadAllAsync(token).ConfigureAwait(false))
                messages.Add(msg);
        }
    }
}