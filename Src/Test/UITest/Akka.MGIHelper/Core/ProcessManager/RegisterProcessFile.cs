using System.Collections.Immutable;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed record StartProcessTracking(ImmutableList<string> FileNames, int ClientAffinity, int OperatingAffinity, ImmutableList<string> PriorityProcesses);
}