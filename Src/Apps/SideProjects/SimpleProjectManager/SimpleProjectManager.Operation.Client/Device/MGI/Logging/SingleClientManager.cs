using System.Buffers;
using System.Net.Sockets;
using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class SingleClientManager : ReceiveActor, IDisposable
{
    private readonly ILogger<SingleClientManager> _logger = LoggingProvider.LoggerFactory.CreateLogger<SingleClientManager>();
    private readonly Socket _client;
    private readonly ChannelWriter<IMemoryOwner<byte>> _writer;

    public SingleClientManager(Socket client, ChannelWriter<LogInfo> writer)
    {
        _client = client;
        _writer = writer;

        Receive<Status.Failure>(OnFailure);
    }

    protected override void PreStart()
    {
        Run().PipeTo(Self).Ignore();
        base.PreStart();
    }

    private void OnFailure(Status.Failure obj)
    {
        ReceiveError(obj.Cause);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error on Recive Data")]
    private partial void ReceiveError(Exception ex);

    private async Task Run()
    {
        try
        {
            while (!_client.Poll(1000, SelectMode.SelectRead) || _client.Available != 0)
            {
                var msg = await OnReceiveAsync().ConfigureAwait(false);

                if(msg is null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                    continue;
                }

                await _writer.WriteAsync(msg).ConfigureAwait(false);
            }
        }
        catch(ObjectDisposedException){}
    }

    private async ValueTask<IMemoryOwner<byte>?> OnReceiveAsync()
    {
        var memory = MemoryPool<byte>.Shared.Rent(1024);

        int num = await _client.ReceiveAsync(memory.Memory).ConfigureAwait(false);

        if(num == 0)
            memory.Dispose();
        else
            return memory;

        return null;
    }
    
    protected override void PostStop()
    {
        _client.Close();
        base.PostStop();
    }

    public void Dispose()
        => _client.Dispose();
}