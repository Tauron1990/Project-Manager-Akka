using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stl.Channels;
using Stl.Fusion.Bridge;
using Stl.Fusion.Bridge.Messages;
using Stl.Text;

namespace Stl.Fusion.AkkaBridge.Internal
{
    public class AkkaFusionServiceHost : BackgroundService
    {
        public const string BaseChannel = "FusionAkkaServer";

        private readonly ActorSystem _system;
        private readonly IPublisher _publisher;
        private readonly ILogger<AkkaFusionServiceHost> _logger;

        public AkkaFusionServiceHost(ActorSystem system, IPublisher publisher, ILogger<AkkaFusionServiceHost> logger)
        {
            _system = system;
            _publisher = publisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = new AkkaChannel<Symbol>(_system, BaseChannel, _logger, stoppingToken);

            await foreach (var newCLient in server.Reader.ReadAllAsync(stoppingToken)) 
                AttachNewClient(newCLient, stoppingToken);
        }

        private async void AttachNewClient(Symbol symbol, CancellationToken stoppingToken)
        {
            try
            {
                using var stopper = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

                var client = new AkkaChannel<AkkaBridgeMessage>(_system, symbol.Value, _logger, stopper.Token);
                var toAttach = Channel.CreateBounded<BridgeMessage>(16);
                if (!_publisher.ChannelHub.Attach(toAttach))
                    throw new InvalidOperationException("Channel Hub Attaching Failed");

                await Convert(client, toAttach, stopper);
                stopper.Cancel();
            }
            catch(OperationCanceledException) {}
            catch (Exception e)
            {
                _logger.LogError(e, "Error on Akka Fusion Client Handler");
            }
        }

        public static Task Convert(AkkaChannel<AkkaBridgeMessage> akka, Channel<BridgeMessage> fusion, CancellationTokenSource stopper)
        {
            var reader = Task.Run(
                async () =>
                {
                    await foreach (var (killClient, bridgeMessage) in akka.Reader.ReadAllAsync(stopper.Token))
                    {
                        if (killClient || bridgeMessage == null)
                        {
                            stopper.Cancel();

                            continue;
                        }
                            
                        await fusion.Writer.WriteAsync(bridgeMessage, stopper.Token);
                    }
                }, stopper.Token);
                
            var writer = Task.Run(
                async () =>
                {
                    await foreach (var msg in fusion.Reader.ReadAllAsync(stopper.Token))
                        await akka.Writer.WriteAsync(new AkkaBridgeMessage(false, msg), stopper.Token);
                    await akka.Writer.WriteAsync(new AkkaBridgeMessage(true, null), stopper.Token);
                }, stopper.Token);
            
            return Task.WhenAll(reader, writer);
        }
    }
}