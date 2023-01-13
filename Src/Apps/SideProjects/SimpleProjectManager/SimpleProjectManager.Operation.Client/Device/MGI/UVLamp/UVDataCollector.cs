using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Actors;
using Akka.Streams.Dsl;
using SimpleProjectManager.Operation.Client.Device.MGI.UVLamp.Events;

namespace SimpleProjectManager.Operation.Client.Device.MGI.UVLamp;

public sealed class UVDataCollector : ReceiveActor, IWithTimers
{
    private readonly Sink<TickEvent, NotUsed> _output;
    private readonly ActorMaterializer _materializer;
    
    public UVDataCollector(Sink<TickEvent, NotUsed> output)
    {
        _output = output;
        _materializer = Context.Materializer();
    }

    protected override void PreStart()
    {
        var source = Source.ActorPublisher<ClockEvent>(Props.Create<ClockPublisher>());

        var actorSink = Sink.ActorSubscriber<TickEvent>(Props.Create<TickReceiver>(Context.Self));
        var sink = Sink.Combine(i => new Broadcast<TickEvent>(i), 
            _output, 
            actorSink.PreMaterialize(_materializer).Item2);

        
        
        base.PreStart();
    }

    public ITimerScheduler Timers { get; set; } = null!;
    
    private sealed class ClockPublisher : ActorPublisher<ClockEvent>
    {
        protected override bool Receive(object message)
        {
            switch (message)
            {
                case ClockEvent evt:
                    OnNext(evt);

                    return true;
                
            }

            return false;
        }
    }
    
    private sealed class TickReceiver : ActorSubscriber
    {
        private readonly IActorRef _trigger;

        public TickReceiver(IActorRef trigger)
        {
            _trigger = trigger;

        }
        
        protected override bool Receive(object message)
        {
            _trigger.Tell(message);
            return true;
        }

        public override IRequestStrategy RequestStrategy { get; } = new WatermarkRequestStrategy(1);
    }
}