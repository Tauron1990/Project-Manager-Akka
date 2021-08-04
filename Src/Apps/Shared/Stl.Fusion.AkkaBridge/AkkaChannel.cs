using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Microsoft.Extensions.Logging;

namespace Stl.Fusion.AkkaBridge
{
    public sealed class AkkaChannel<TMessage> : Channel<TMessage>
    {
        public AkkaChannel(ActorSystem system, string channel, ILogger logger, CancellationToken token)
        {
            var reader = Channel.CreateBounded<TMessage>(16);
            var writer = Channel.CreateBounded<TMessage>(16);
            system.ActorOf(Props.Create(() => new ChannelActor(writer, reader, channel, logger, token)));

            Reader = reader;
            Writer = writer;

            token.Register(() => writer.Writer.TryComplete());
        }

        private sealed class ChannelActor : ReceiveActor
        {
            public ChannelActor(ChannelReader<TMessage> reader, ChannelWriter<TMessage> writer, string channel, ILogger logger, CancellationToken token)
            {
                void Next()
                {
                    if(token.IsCancellationRequested) return;
                    reader.ReadAsync(token)
                       .AsTask().PipeTo(Self, success:m => new Status.Success(m));
                }

                var distributedPubSub = DistributedPubSub.Get(Context.System);
                
                Receive<Status.Failure>(s =>
                                        {
                                            logger.LogError(s.Cause, "Error on Process Message");
                                            Next();
                                        });
                
                Receive<Status.Success>(m =>
                                  {
                                      distributedPubSub.Mediator.Tell(new Publish(channel, m.Status));
                                      Next();
                                  });
                
                distributedPubSub.Mediator
                   .Ask<SubscribeAck>(new Subscribe(channel, Self), TimeSpan.FromSeconds(20), token)
                   .PipeTo(Self);
                
                Receive<SubscribeAck>(_ => {});
                
                Receive<TMessage>(m =>
                                  {
                                      while(!writer.TryWrite(m) && !token.IsCancellationRequested) {}
                                  });

                Next();
            }
        }
    }
}