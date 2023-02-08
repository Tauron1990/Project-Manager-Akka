using System.Buffers;
using System.Net.Sockets;
using System.Text;
using AutoFixture;
using SimpleProjectManager.Operation.Client.Device.MGI;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

namespace SimpleProjectManager.Operation.Client.Tests.IntegrationTest.Device.MGI.Logging;

public sealed class LogsGenerator
{
    private readonly int _clientCount;
    private readonly int _messageCount;
    private readonly CancellationToken _cancellationToken;
    private readonly Fixture _fixture = new();
    private readonly Random _random = new(111);
    
    public LogsGenerator(int clientCount, int messageCount, CancellationToken cancellationToken)
    {
        _clientCount = clientCount;
        _messageCount = messageCount;
        _cancellationToken = cancellationToken;
    }

    public async Task DoWrite()
    {
        string clientName = $"Client {_fixture.Create<string>()} ";

#pragma warning disable MA0076
        await Task.WhenAll(Enumerable.Range(1, _clientCount).Select(i => RunClient($"{clientName}{i}", i))).ConfigureAwait(false);
#pragma warning restore MA0076
    }

    private async Task RunClient(string name, int counter)
    {
        using var client = new TcpClient();

        await Task.Delay(counter * 1000, _cancellationToken).ConfigureAwait(true);
        
        await client.ConnectAsync("127.0.0.1", MgiMachine.Port, _cancellationToken).ConfigureAwait(false);
        
#pragma warning disable MA0004
        await using NetworkStream stream = client.GetStream();
#pragma warning restore MA0004

        await Write(
                stream,
                new LoggerTcpFormat.Message(
                    LoggerTcpFormat.MessageType.Command,
                    new LoggerTcpFormat.Command(LoggerTcpFormat.CommandType.SetApp, name)))
            .ConfigureAwait(false);
        
        
        
        foreach (int count in Enumerable.Range(1, _messageCount))
        {
            if(count % 1000 == 0)
                await Task.Delay(1000, _cancellationToken).ConfigureAwait(false);
            
            var info = new LogInfo(
                DateTime.Now,
                name,
                GetLogType(),
                $"Message -- {_fixture.Create<string>()}: {count}",
                Command.Log);

            //await Task.Delay(_random.Next(0, 100)).ConfigureAwait(false);
            
            await Write(stream, LoggerTcpFormat.GetTcpable(info)).ConfigureAwait(false);
        }

        await Write(
                stream,
                new LoggerTcpFormat.Message(
                    LoggerTcpFormat.MessageType.Command, 
                    new LoggerTcpFormat.Command(LoggerTcpFormat.CommandType.Disconnect)))
            .ConfigureAwait(false);
        
        client.Close();
    }

    private string GetLogType() => ((LoggerTcpFormat.LogType)_random.Next(0, 9)).ToString();

    private async ValueTask Write(NetworkStream stream, LoggerTcpFormat.ITcpable tcpable)
    {
        using var mem = MemoryPool<byte>.Shared.Rent();

        int count = Encoding.UTF8.GetBytes(tcpable.ToTcp(), mem.Memory.Span);

        await stream.WriteAsync(mem.Memory[..count], _cancellationToken).ConfigureAwait(false);
    }
}