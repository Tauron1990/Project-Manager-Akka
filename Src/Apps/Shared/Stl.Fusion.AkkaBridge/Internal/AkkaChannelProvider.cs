using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Microsoft.Extensions.Logging;
using Stl.Fusion.Bridge;
using Stl.Fusion.Bridge.Messages;
using Stl.Text;

namespace Stl.Fusion.AkkaBridge.Internal
{
    public class AkkaChannelProvider : IChannelProvider
    {
        private readonly ActorSystem _system;
        private readonly ILogger<AkkaChannelProvider> _logger;

        public AkkaChannelProvider(ActorSystem system, ILogger<AkkaChannelProvider> logger)
        {
            _system = system;
            _logger = logger;
        }
        
        public async Task<Channel<BridgeMessage>> CreateChannel(Symbol publisherId, CancellationToken cancellationToken)
        {
            var _ = await DistributedPubSub.Get(_system).Mediator.Ask<Status.Success>(new Publish(AkkaFusionServiceHost.BaseChannel, publisherId));

            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            source.Token.Register(() => source.Dispose());
            
            var server = new AkkaChannel<AkkaBridgeMessage>(_system, publisherId.Value, _logger, cancellationToken, false);
            var toReturn = Channel.CreateBounded<BridgeMessage>(16);

            AkkaFusionServiceHost.Convert(server, toReturn, source)
                                 .ContinueWith(
                                      t =>
                                      {
                                          if(t.IsFaulted)
                                              _logger.LogError(t.Exception, "Error on Converting Brige Message");
                                      });

            return toReturn;
        }
    }
}