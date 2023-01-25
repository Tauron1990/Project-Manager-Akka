using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class SingleClientManager : ReceiveActor, IDisposable
{
    private readonly ILogger<SingleClientManager> _logger = LoggingProvider.LoggerFactory.CreateLogger<SingleClientManager>();
    private readonly IMessageStream _client;
    private readonly ChannelWriter<LogInfo> _writer;
    private readonly LogParser _logParser;

    private string _app = string.Empty;

    public SingleClientManager(IMessageStream client, ChannelWriter<LogInfo> writer)
    {
        _client = client;
        _writer = writer;
        _logParser = new LogParser(client);
        
        Receive<ClientError>(OnFailure);
        
        Run(Self).PipeTo(Self,
            success:() => PoisonPill.Instance,
            failure: e => ClientError.CreateError("Error on Process Socket Receive Data", e));
    }

    private void OnFailure(ClientError obj)
    {
        ReceiveError(obj.Error, obj.Message);
        Context.Stop(Self);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error on Recive Data. {message}")]
    private partial void ReceiveError(Exception? ex, string message);

    private async Task Run(IActorRef self)
    {
        var logs = Channel.CreateBounded<LogInfo>(1000);
        Task runner = _logParser.Run(logs.Writer);
        
        try
        {
            await foreach (LogInfo infoItem in logs.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                LogInfo info = infoItem;
                
                if(string.IsNullOrWhiteSpace(info.Application))
                    info = info with { Application = _app };

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (info.Command)
                {
                    case Command.Disconnect:
                        await _writer.WriteAsync(info).ConfigureAwait(false);
                        return;
                    case Command.SetApp:
                        _app = info.Content;
                        break;
                }

                await _writer.WriteAsync(info).ConfigureAwait(false);
            }
        }
        catch (ObjectDisposedException) { }
        catch (Exception e)
        {
            self.Tell(ClientError.CreateError("Error on Process Socket Receive Data", e));
        }

        await runner.ConfigureAwait(false);
    }
    
    protected override void PostStop()
    {
        _logParser.Dispose();
        base.PostStop();
    }

    public void Dispose()
        => _client.Dispose();


    private record ClientError(Exception? Error, string Message)
    {
        internal static ClientError CreateError(string message, Exception? error = null)
            => new(error, message);
    }
}