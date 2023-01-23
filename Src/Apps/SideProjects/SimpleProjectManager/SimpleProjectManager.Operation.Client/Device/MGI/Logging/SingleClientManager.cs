using System.Buffers;
using System.Net.Sockets;
using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class SingleClientManager : ReceiveActor, IDisposable
{
    private static readonly Tick TickInstance = new();
    
    private readonly ILogger<SingleClientManager> _logger = LoggingProvider.LoggerFactory.CreateLogger<SingleClientManager>();
    private readonly ISocket _client;
    private readonly ChannelWriter<LogInfo> _writer;
    private readonly LogParser _logParser = new();

    private int _errorCount;
    private string _app = string.Empty;

    public SingleClientManager(ISocket client, ChannelWriter<LogInfo> writer)
    {
        _client = client;
        _writer = writer;

        Receive<ClientError>(OnFailure);
        
        ReceiveAsync<Tick>(async _ => Self.Tell(await Run().ConfigureAwait(false)));
    }

    protected override void PreStart()
    {
        Self.Tell(TickInstance);
        base.PreStart();
    }

    private void OnFailure(ClientError obj)
    {
        ReceiveError(obj.Error, obj.Message);
        Context.Stop(Self);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error on Recive Data. {message}")]
    private partial void ReceiveError(Exception? ex, string message);

    private async Task<object> Run()
    {
        try
        {
            if(!IsConected())
            {
                _errorCount++;
                await Task.Delay(1000).ConfigureAwait(false);

                return TickInstance;
            }

            if(_errorCount > 5)
                return ClientError.CreateError("Invalid Socket Connection");
            
            var msg = await OnReceiveAsync().ConfigureAwait(false);

            if(msg.Data is null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);

                return TickInstance;
            }

            LogInfo? info = _logParser.Push(msg.Data, msg.lenght);

            if(info is null)
            {
                return TickInstance;
            }

            if(string.IsNullOrWhiteSpace(info.Application))
                info = info with { Application = _app };

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (info.Command)
            {
                case Command.Disconnect:
                    await _writer.WriteAsync(info).ConfigureAwait(false);

                    return PoisonPill.Instance;
                case Command.SetApp:
                    _app = info.Content;

                    break;
            }

            await _writer.WriteAsync(info).ConfigureAwait(false);

            return TickInstance;
        }
        catch (ObjectDisposedException)
        {
            return PoisonPill.Instance;
        }
        catch (Exception e)
        {
            return ClientError.CreateError("Error on Process Socket Receive Data", e);
        }
    }

    private bool IsConected()
        => !_client.Poll(1000, SelectMode.SelectRead) || _client.Available != 0;

    private async ValueTask<(IMemoryOwner<byte>? Data, int lenght)> OnReceiveAsync()
    {
        var memory = MemoryPool<byte>.Shared.Rent(1024);

        int num = await _client.ReceiveAsync(memory.Memory).ConfigureAwait(false);

        if(num == 0)
            memory.Dispose();
        else
            return (memory, num);

        return default;
    }
    
    protected override void PostStop()
    {
        _client.Close();
        _logParser.Dispose();
        base.PostStop();
    }

    public void Dispose()
        => _client.Dispose();

    private record Tick;

    private record ClientError(Exception? Error, string Message)
    {
        internal static ClientError CreateError(string message, Exception? error = null)
            => new(error, message);
    }
}