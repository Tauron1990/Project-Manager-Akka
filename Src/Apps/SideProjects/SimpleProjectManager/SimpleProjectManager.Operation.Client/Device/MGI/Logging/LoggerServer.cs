using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Akka.Actor;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed class LoggerServer : ReceiveActor, IDisposable
{
    private readonly ChannelWriter<LogInfo> _logSink;
    private readonly Socket _server;
    private readonly IPEndPoint _endPoint;

    public LoggerServer(ChannelWriter<LogInfo> logSink, int port)
    {
        _logSink = logSink;
        
        IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        _endPoint = new IPEndPoint(iPAddress, port);
        _server = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    private void Running()
    {
        Context.ActorOf(Props.Create(() => new AcceptManager(_server)));

        Receive<Socket>(
            newSocked => Context.ActorOf(Props.Create(() => new SingleClientManager(new SocketMessageStream(newSocked), _logSink))));
    }

    private void OnFailure(Exception error)
    {
        _logSink.TryComplete(error);
        
        Self.Tell(PoisonPill.Instance);
    }

    protected override void PreStart()
    {
        try
        {
            _server.Bind(_endPoint);
            _server.Listen(100);
            
            Become(Running);
        }
        catch (Exception e)
        {
            OnFailure(e);
        }

        base.PreStart();
    }

    protected override void PostStop()
    {
        _server.Close();
        _logSink.TryComplete();
        base.PostStop();
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}