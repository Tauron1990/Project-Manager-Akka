using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron.Features;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class SingleClientManager : ActorFeatureBase<SingleClientManager.State>
{
    public sealed record State(IMessageStream Client, ChannelWriter<LogInfo> Writer, LogParser LogParser, string App = "Unkowen") : IDisposable
    {
        public void Deconstruct(out ChannelWriter<LogInfo> writer, out LogParser logparser)
        {
            writer = Writer;
            logparser = LogParser;
        }

        public void Dispose()
        {
            Client.Dispose();
            LogParser.Dispose();
        }
    }

    private sealed record NewName(string Name);
    
    private readonly ILogger _logger;

    public static IPreparedFeature New(IMessageStream client, ChannelWriter<LogInfo> writer)
        => Feature.Create(() => new SingleClientManager(), _ => new State(client, writer, new LogParser(client)));

    private SingleClientManager() => _logger = Logger;

    protected override void ConfigImpl()
    {
        Receive<ClientError>(OnFailure);
        ReceiveState<NewName>(p => p.State with { App = p.Event.Name });
        
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
    private static async Task Run(State state, IActorRef self)
    {
        using var source = new CancellationTokenSource();

        (var writer, LogParser logParser) = state;
        
        var logs = Channel.CreateBounded<LogInfo>(1000);
        Task runner = logParser.Run(logs.Writer, source.Token);

        try
        {
            await foreach (LogInfo infoItem in logs.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                LogInfo info = infoItem;

                if(string.IsNullOrWhiteSpace(info.Application))
                    info = info with { Application = state.App };

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (info.Command)
                {
                    case Command.Disconnect:
                        await writer.WriteAsync(info, source.Token).ConfigureAwait(false);

                        return;
                    case Command.SetApp:
                        state = state with { App = info.Content };
                        self.Tell(new NewName(info.Content));
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
    
    public void Dispose()
        => CurrentState.Dispose();


    private record ClientError(Exception? Error, string Message)
    {
        internal static ClientError CreateError(string message, Exception? error = null)
            => new(error, message);
    }
}