using System.Diagnostics;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed record ProcessExitMessage(Process Target, string Name, int Id);
}