using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using SimpleProjectManager.Operation.Client.Device.MGI.UVLamp.Events;

namespace SimpleProjectManager.Operation.Client.Device.MGI.UVLamp;

public sealed class UVDataCollector : ReceiveActor
{
    private readonly Sink<TickEvent, NotUsed> _output;
    private readonly ActorMaterializer _materializer;
    
    public UVDataCollector(Sink<TickEvent, NotUsed> output)
    {
        _output = output;
        _materializer = Context.Materializer();
    }
}