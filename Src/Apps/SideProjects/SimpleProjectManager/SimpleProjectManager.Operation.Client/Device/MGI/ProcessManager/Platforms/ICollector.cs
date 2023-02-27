using System.Runtime.InteropServices;
using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager.Platforms;

public interface ICollector : IDisposable
{
    public static ICollector Create(IActorRef owner, ILogger logger)
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new Win32Collector(owner, logger);

        return new GenericCollector(owner, logger);
    }
}