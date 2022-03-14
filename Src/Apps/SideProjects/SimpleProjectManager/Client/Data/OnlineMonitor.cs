using System.Reactive.Linq;
using Microsoft.JSInterop;
using SimpleProjectManager.Shared.ServerApi.RestApi;

namespace SimpleProjectManager.Client.Data;

public sealed class OnlineMonitor : IOnlineMonitor
{
    private readonly Task<IJSObjectReference> _jsRuntime;
    private readonly ILogger<OnlineMonitor> _logger;
    private readonly IPingServiceDef _pingService;
    
    public IObservable<bool> Online { get; }

    public OnlineMonitor(HttpClient client, IJSRuntime jsRuntime, ILogger<OnlineMonitor> logger)
    {
        _jsRuntime = jsRuntime.InvokeAsync<IJSObjectReference>("Index").AsTask();
        _logger = logger;
        _pingService = RestEase.RestClient.For<IPingServiceDef>(client);
        Online =
            Observable.Interval(TimeSpan.FromSeconds(6))
               .SelectMany(_ => IsOnline())
               .StartWith(true)
               .DistinctUntilChanged();
    }

    public async Task<bool> IsOnline()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            try
            {
                var refernce = await _jsRuntime;
                if (!await refernce.InvokeAsync<bool>("isOnline", source.Token))
                    return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on Invoke IsOnline JS Function");
            }
            return await _pingService.Ping(source.Token) == "ok";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on Ping Server");
            return false;
        }
    }
}