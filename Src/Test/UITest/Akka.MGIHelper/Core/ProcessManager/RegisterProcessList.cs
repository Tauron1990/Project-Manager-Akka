using Akka.Actor;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed record RegisterProcessList(IActorRef Intrest, StartProcessTracking TrackingData);
}