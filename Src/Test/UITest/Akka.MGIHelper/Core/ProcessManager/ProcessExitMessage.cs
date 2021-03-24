using System.Diagnostics;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed record ProcessExitMessage(Process Target, string Name, int Id);
}