using Akka.Actor;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed record RegisterProcessList(IActorRef Intrest, StartProcessTracking TrackingData);
}