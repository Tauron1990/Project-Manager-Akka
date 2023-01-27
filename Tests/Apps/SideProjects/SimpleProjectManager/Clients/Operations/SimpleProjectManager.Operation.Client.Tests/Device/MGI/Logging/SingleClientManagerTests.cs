using System.Threading.Channels;
using Akka.TestKit.Xunit;
using FluentAssertions;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

namespace SimpleProjectManager.Operation.Client.Tests.Device.MGI.Logging;

public sealed class SingleClientManagerTests : TestKit
{
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TestSingleClient(bool multi)
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(10000));
        
        var channel = Channel.CreateUnbounded<LogInfo>();

        ActorOf(() => new SingleClientManager(multi ? TestData.Multi() : TestData.Single(), channel.Writer));

        var result = await channel.Reader.ReadAllAsync(source.Token).ToListAsync(source.Token).ConfigureAwait(false);

        result.Count.Should().Be(multi ? 6 : 1);
    }
}