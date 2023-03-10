using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Features;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class SingleClientManager : ActorFeatureBase<SingleClientManager.State>
{
    public readonly record struct State(IMessageStream Client, ChannelWriter<LogInfo> Writer, LogParser LogParser, string App) : IDisposable
    {
        public void Dispose() => LogParser.Dispose();
    }

    public static IPreparedFeature New(IMessageStream client, ChannelWriter<LogInfo> writer)
        => Feature.Create(
            () => new SingleClientManager(),
            _ => new State(client, writer, new LogParser(client), App: "Unkowen"));
    
    private readonly ILogger _logger;

    private SingleClientManager()
    {
        _logger = Logger;
    }

    protected override void ConfigImpl()
    {
        Stop.ToUnit(CurrentState.LogParser.Dispose).Subscribe();
        CurrentState.Client.DisposeWith(this);
        
        Receive<ClientError>(OnFailure);
        
        Run(CurrentState, Self).PipeTo(Self,
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

    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    private async Task Run(State state, IActorRef self)
    {
        LogParser logParser = state.LogParser;
        string app = state.App;
        var writer = state.Writer;
        
        using var source = new CancellationTokenSource(); 
        
        var logs = Channel.CreateBounded<LogInfo>(1000);
        Task runner = logParser.Run(logs.Writer, source.Token);

        try
        {
            await foreach (LogInfo infoItem in logs.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                LogInfo info = infoItem;

                if(string.IsNullOrWhiteSpace(info.Application))
                    info = info with { Application = app };

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (info.Command)
                {
                    case Command.Disconnect:
                        await writer.WriteAsync(info, source.Token).ConfigureAwait(false);

                        return;
                    case Command.SetApp:
                        app = info.Content;

                        break;
                }

                await writer.WriteAsync(info, source.Token).ConfigureAwait(false);
            }
        }
        catch (ObjectDisposedException) { }
        catch (Exception e)
        {
            self.Tell(ClientError.CreateError("Error on Process Socket Receive Data", e));
        }
        finally
        {
            writer.TryComplete();
            source.Cancel();
        }
        
        await runner.ConfigureAwait(false);
    }


    private record ClientError(Exception? Error, string Message)
    {
        internal static ClientError CreateError(string message, Exception? error = null)
            => new(error, message);
    }
}