using System;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Actors;
using Akka.Streams.Dsl;
using SimpleProjectManager.Operation.Client.Device.MGI.UVLamp.Events;

namespace SimpleProjectManager.Operation.Client.Device.MGI.UVLamp;

public sealed class UvDataCollector : ReceiveActor, IWithTimers
{
    private static readonly TickEvent Tick = new TickEvent();
    
    private readonly Sink<TrackingEvent, NotUsed> _output;
    private readonly MgiOptions _options;
    private readonly ActorMaterializer _materializer;
    private readonly DataFetcher _dataFetcher;
    
    private IActorRef _sender = Nobody.Instance;
    
    public UvDataCollector(Sink<TrackingEvent, NotUsed> output, MgiOptions options)
    {
        _output = output;
        _options = options;
        _materializer = Context.Materializer();
        _dataFetcher = new DataFetcher(options);

        Receive<TrackingEvent>(StartNext);
        Receive<TickEvent>(TriggerNext);
    }

    private void TriggerNext(TickEvent obj)
        => _sender.Tell(obj);

    private void StartNext(TrackingEvent obj)
        => Timers.StartSingleTimer(obj, Tick, TimeSpan.FromMilliseconds(_options.ClockTimeMs));

    protected override void PreStart()
    {
        var source = Source.ActorPublisher<TickEvent>(Props.Create<ClockPublisher>());

        var actorSink = Sink.ActorSubscriber<TrackingEvent>(Props.Create<TickReceiver>(Context.Self));
        var sink = Sink.Combine(i => new Broadcast<TrackingEvent>(i), 
            _output, 
            actorSink.PreMaterialize(_materializer).Item2);

        var senderPair = source.PreMaterialize(_materializer);
        _sender = senderPair.Item1;

        senderPair.Item2
           .SelectAsync(1, e => _dataFetcher.Handle(e))
           .RunWith(sink, _materializer);

        _sender.Tell(Tick);
        
        base.PreStart();
    }

    protected override void PostStop()
    {
        _dataFetcher.Dispose();
        base.PostStop();
    }

    public ITimerScheduler Timers { get; set; } = null!;
    
    private sealed class ClockPublisher : ActorPublisher<TickEvent>
    {
        protected override bool Receive(object message)
        {
            switch (message)
            {
                case TickEvent evt:
                    OnNext(evt);

                    return true;
                
            }

            return false;
        }
    }
    
    private sealed class TickReceiver : ActorSubscriber
    {
        private readonly IActorRef _trigger;

        #pragma warning disable GU0073
        public TickReceiver(IActorRef trigger)
            #pragma warning restore GU0073
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