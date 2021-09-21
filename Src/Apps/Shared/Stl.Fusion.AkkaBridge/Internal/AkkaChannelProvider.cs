using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Microsoft.Extensions.Logging;
using Stl.Fusion.AkkaBridge.Connector.Data;
using Stl.Fusion.Bridge;
using Stl.Fusion.Bridge.Messages;
using Stl.Text;
using Tauron;

namespace Stl.Fusion.AkkaBridge.Internal
{
    public class AkkaChannelProvider : IChannelProvider
    {
        private readonly TimeSpan _delay;
        private readonly ILogger<AkkaChannelProvider> _logger;
        private readonly ActorSystem _system;

        public AkkaChannelProvider(ActorSystem system, ILogger<AkkaChannelProvider> logger)
        {
            _system = system;
            _logger = logger;
            _delay = DistributedPubSubSettings.Create(system).GossipInterval * 2;
        }

        public async Task<Channel<BridgeMessage>> CreateChannel(Symbol publisherId, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
            var mediator = DistributedPubSub.Get(_system).Mediator;
            var _ = await mediator.Ask<Status.Success>(new Publish(AkkaFusionServiceHost.BaseChannel, new SymbolData(publisherId)));

            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            source.Token.Register(() => source.Dispose());

            var server = new AkkaChannel<AkkaBridgeMessage>(_system, publisherId.Value, _logger, cancellationToken, false);
            var toReturn = Channel.CreateBounded<BridgeMessage>(16);

            AkkaFusionServiceHost.Convert(server, toReturn, source)
               .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                            _logger.LogError(t.Exception, "Error on Converting Brige Message");
                    })
               .Ignore();

            return toReturn;
        }
    }
}