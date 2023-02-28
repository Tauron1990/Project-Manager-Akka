using System.Collections.Immutable;
using System.Diagnostics;
using System.Timers;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;
using Timer = System.Timers.Timer;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager.Platforms;

public sealed partial class GenericCollector : ICollector
{
    private readonly object _sync = new();
    private readonly IActorRef _owner;
    private readonly ILogger _logger;
    private readonly Timer _timer;
    private ImmutableDictionary<int, ProcessData> _monitored = ImmutableDictionary<int, ProcessData>.Empty;

    public GenericCollector(IActorRef owner, ILogger logger)
    {
        _owner = owner;
        _logger = logger;

        _timer = new Timer(TimeSpan.FromSeconds(2))
        {
            AutoReset = true,
        };
        
        _timer.Elapsed += OnGartherProcess;
    }

    private void OnGartherProcess(object? sender, ElapsedEventArgs e)
    {
        lock (_sync)
        {
            var original = Interlocked.Exchange(ref _monitored, ImmutableDictionary<int, ProcessData>.Empty);

            foreach (ProcessData process in original.Values)
            {
                if(process.HasExited)
                {
                    original = original.Remove(process.Id);
                    process.Dispose();
                }
            }

            try
            {
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    var data = new ProcessData(process.Id, process);
                    if(original.ContainsKey(data.Id))
                        data.Dispose();
                    else
                    {
                        original = original.Add(data.Id, data);
                        _owner.Tell(process);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorOnUpdateMonitoredProcesses(exception);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, EventId = 1, Message = "Error on Get Process from Event")]
    private partial void ErrorOnUpdateMonitoredProcesses(Exception e);
    
    public void Dispose()
    {
        _timer.Dispose();
        _monitored.Foreach(d => d.Value.Dispose());
    }

    private readonly record struct ProcessData(int Id, Process Process) : IDisposable
    {
        internal bool HasExited
        {
            get
            {
                Process.Refresh();
                return Process.HasExited;
            }
        }

        public void Dispose() => Process.Dispose();
    }
}