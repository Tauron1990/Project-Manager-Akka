using System.Collections.Immutable;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed record StartProcessTracking(ImmutableList<string> FileNames);
}