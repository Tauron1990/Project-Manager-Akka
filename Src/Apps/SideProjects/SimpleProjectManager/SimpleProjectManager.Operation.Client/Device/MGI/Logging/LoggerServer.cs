using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Akka.Actor;
using Tauron;
using Tauron.Features;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed class LoggerServer : ActorFeatureBase<(ChannelWriter<LogInfo> LogSink, Socket Server, IPEndPoint EndPoint)>
{
    public static Func<MgiLoggingConfiguration, IPreparedFeature> New(ChannelWriter<LogInfo> logSink)
        => config =>
        {
            IPAddress iPAddress = IPAddress.Parse(config.Ip);
            var endPoint = new IPEndPoint(iPAddress, config.Port);
            var server = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            return Feature.Create(
                () => new LoggerServer(),
                (logSink, server, endPoint)
            );
        };

    private LoggerServer() { }
    
    protected override void ConfigImpl()
    {
        Start.Subscribe(OnStart);
        Stop.Subscribe(OnStop);
        
        ReceiveState<Socket>(NewClient);
    }

    private void OnFailure(Exception error)
    {
        CurrentState.LogSink.TryComplete(error);
        
        Self.Tell(PoisonPill.Instance);
    }
    
    private void OnStart(IActorContext context)
    {
        CurrentState.Server.DisposeWith(this);
        
        try
        {
            Socket server = CurrentState.Server;
            IPEndPoint endPoint = CurrentState.EndPoint;
            
            server.Bind(endPoint);
            server.Listen(100);

            context.ActorOf(AcceptManager.New(server));
        }
        catch (Exception e)
        {
            OnFailure(e);
        }
    }

    private void OnStop(IActorContext _)
    {
        CurrentState.Server.Close();
        CurrentState.LogSink.TryComplete();
    }
    
    private static void NewClient(StatePair<Socket, (ChannelWriter<LogInfo> LogSink, Socket Server, IPEndPoint EndPoint)> evt) => 
        evt.Context.ActorOf(SingleClientManager.New(new SocketMessageStream(evt.Event), evt.State.LogSink));
}