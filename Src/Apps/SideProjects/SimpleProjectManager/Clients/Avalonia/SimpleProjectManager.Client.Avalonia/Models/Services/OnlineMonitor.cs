using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Shared.Data;

namespace SimpleProjectManager.Client.Avalonia.Models.Services;

public sealed class OnlineMonitor : OnlineMonitorBase<OnlineMonitor>
{
    public OnlineMonitor(HttpClient client, ILogger<OnlineMonitor> logger) 
        : base(client, logger) { }
    protected override ValueTask<bool> RunInternal(CancellationToken token)
        => ValueTask.FromResult(true);
}