using System.Threading.Tasks;
using Akka.MGIHelper.Core.FanControl.Bus;
using SimpleProjectManager.Operation.Client.Device.MGI.UVLamp.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class PowerComponent : IHandler<TrackingEvent>
    {
        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if (msg.State == State.Power)
                await messageBus.Publish(new FanStartEvent());
        }
    }
}