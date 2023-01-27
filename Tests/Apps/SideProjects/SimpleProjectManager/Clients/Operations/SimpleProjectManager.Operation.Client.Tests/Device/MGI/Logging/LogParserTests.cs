using System.Threading.Channels;
using FluentAssertions;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

namespace SimpleProjectManager.Operation.Client.Tests.Device.MGI.Logging;

public sealed class LogParserTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LogParserTest(bool multi)
    {
        using var parser = new LogParser(
            multi 
            ? TestData.Multi()
            : TestData.Single());
        var info = Channel.CreateUnbounded<LogInfo>();

        Task runner = parser.Run(info.Writer, default);

        var list = await info.Reader.ReadAllAsync().ToListAsync();

        await runner;

        list.Count.Should().Be(multi ? 6 : 1);
    }
}