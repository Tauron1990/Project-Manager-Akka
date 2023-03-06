using System.Net.Sockets;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Features;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed partial class AcceptManager : ActorFeatureBase<Socket>
{
    public static IPreparedFeature New(Socket server)
        => Feature.Create(() => new AcceptManager(), server);
    
    
    private readonly ILogger _logger;
    
    private AcceptManager()
    {
        _logger = Logger;
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
            if(!CurrentState.IsBound) return;

            CurrentState.AcceptAsync()
               .PipeTo(Self)
               .Ignore();
        }
        catch (ObjectDisposedException)
        {
            
        }
    }

    protected override void ConfigImpl()
    {
        Receive<Status.Failure>(AcceptFailed);
        Receive<Socket>(AcceptOk);
        
        OnAccept();
    }
}