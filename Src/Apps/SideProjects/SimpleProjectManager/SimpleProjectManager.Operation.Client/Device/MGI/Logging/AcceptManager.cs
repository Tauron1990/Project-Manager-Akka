using System.Net.Sockets;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class AcceptManager : ReceiveActor
{
    private readonly Socket _server;
    private readonly ILogger<AcceptManager> _logger;

    public AcceptManager(Socket server)
    {
        _server = server;
        _logger = LoggingProvider.LoggerFactory.CreateLogger<AcceptManager>();
        
        Receive<Status.Failure>(AcceptFailed);
        Receive<Socket>(AcceptOk);
        
        OnAccept();
    }

    private void AcceptOk(Socket newClint)
    {
        try
        {
            Context.Parent.Tell(newClint);
        }
        catch (Exception e)
        {
            AcceptError(e);
        }
        
        OnAccept();
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error on Accept Client")]
    private partial void AcceptError(Exception error);
    
    private void AcceptFailed(Status.Failure obj)
    {
        AcceptError(obj.Cause);
        OnAccept();
    }

    private void OnAccept()
    {
        try
        {
            if(!_server.IsBound) return;

            _server.AcceptAsync()
               .PipeTo(Self)
               .Ignore();
        }
        catch (ObjectDisposedException)
        {
            
        }
    }

}