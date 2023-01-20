using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed class LoggerClient : ReceiveActor, IDisposable
{
    private readonly Sink<LogInfo, NotUsed> _logSink;
    private readonly ActorMaterializer _materializer;
    private readonly Socket _server;
    private readonly IPEndPoint _endPoint;
    private readonly Channel<LogInfo> _channel;

    public LoggerClient(Sink<LogInfo, NotUsed> logSink, int port)
    {
        _logSink = logSink;
        _materializer = Context.Materializer();
        
        IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        _endPoint = new IPEndPoint(iPAddress, port);
        _server = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        _channel = Channel.CreateUnbounded<LogInfo>();
    }

    private void Running()
    {
        var source = Source.ChannelReader(_channel.Reader);

        source.RunWith(_logSink, _materializer);
        
        Context.ActorOf(Props.Create(() => new AcceptManager(_server)));

        Receive<Socket>(
            newSocked => Context.ActorOf(Props.Create(() => new SingleClientManager(new RealSocked(newSocked), _channel.Writer))));
    }

    private void OnFailure(Exception error)
    {
        _logSink.RunWith(
            Source.Failed<LogInfo>(error),
            _materializer
        );
        
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
        _channel.Writer.Complete();
        base.PostStop();
    }

    public void Dispose()
    {
        _materializer.Dispose();
        _server.Dispose();
    }
}