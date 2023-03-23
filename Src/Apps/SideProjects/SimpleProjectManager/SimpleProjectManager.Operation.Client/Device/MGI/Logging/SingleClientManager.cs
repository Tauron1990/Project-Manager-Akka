using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron.Application;
using Tauron.Features;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class SingleClientManager : ActorFeatureBase<SingleClientManager.State>
{
    public sealed record State(IMessageStream Client, ChannelWriter<LogInfo> Writer, LogParser LogParser, string App = "Unkowen");

    private readonly ILogger _logger;

    public static IPreparedFeature New

    private SingleClientManager(IMessageStream client, ChannelWriter<LogInfo> writer)
    {
        _logger = Logger;
        
        _client = client;
        _writer = writer;
        _logParser = new LogParser(client);
        
        Receive<ClientError>(OnFailure);
        
        Run(Self).PipeTo(Self,
            success:() => PoisonPill.Instance,
            failure: e => ClientError.CreateError("Error on Process Socket Receive Data", e));
    }

    protected override void ConfigImpl()
    {
        
    }

    private void OnFailure(ClientError obj)
    {
        ReceiveError(obj.Error, obj.Message);
        Context.Stop(Self);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error on Recive Data. {message}")]
    private partial void ReceiveError(Exception? ex, string message);

    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    private async Task Run(IActorRef self)
    {
        using var source = new CancellationTokenSource(); 
        
        var logs = Channel.CreateBounded<LogInfo>(1000);
        Task runner = _logParser.Run(logs.Writer, source.Token);

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
        finally
        {
            _writer.TryComplete();
            source.Cancel();
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