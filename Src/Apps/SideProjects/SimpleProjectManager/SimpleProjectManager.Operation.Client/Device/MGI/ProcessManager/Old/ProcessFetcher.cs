using System.Diagnostics;
using Akka.Actor;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager.Old;

public sealed partial class ProcessFetcher : ReceiveActor, IWithTimers
{
    private readonly ILogger<ProcessFetcher> _logger;
    
    public ITimerScheduler Timers { get; set; } = null!;

    public ProcessFetcher(ILogger<ProcessFetcher> logger)
    {
        _logger = logger;
        
        Timers.StartPeriodicTimer(nameof(RunFetch), new RunFetch(), TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(2));

        Receive<RunFetch>(GetProcesses);
    }

    [LoggerMessage(Level = LogLevel.Error, EventId = 1, Message = "Error on Get ProcessList")]
    private partial void ErrorOnGetProcess(Exception e);
    
    private void GetProcesses(RunFetch _)
    {
        try
        {
            var processes = Process.GetProcesses();
            Sender.Tell(processes);
        }
        catch (Exception e)
        {
            ErrorOnGetProcess(e);
            throw;
        }
    }

    private sealed record RunFetch;
}