using Microsoft.JSInterop;
using SimpleProjectManager.Client.Shared.Data;

namespace SimpleProjectManager.Client.Data;

public sealed class OnlineMonitor : OnlineMonitorBase<OnlineMonitor>
{
    private readonly IJSRuntime _jsRuntime;


    public OnlineMonitor(HttpClient client, ILogger<OnlineMonitor> logger, IJSRuntime jsRuntime) : base(client, logger)
        => _jsRuntime = jsRuntime;

    protected override  async ValueTask<bool> RunInternal(CancellationToken token)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("window.isOnline", token);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error on Invoke IsOnline JS Function");

            return false;
        }
    }
}