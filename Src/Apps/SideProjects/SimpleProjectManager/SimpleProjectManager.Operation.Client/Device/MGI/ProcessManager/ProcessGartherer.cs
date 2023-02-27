using System.Diagnostics;
using System.Management;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager.Platforms;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager;

public sealed class ProcessGartherer : IDisposable
{
    private readonly ICollector _collector;

    public ProcessGartherer(IActorRef owner, ILogger logger) => _collector = ICollector.Create(owner, logger);

    public void Dispose() => _collector.Dispose();
}