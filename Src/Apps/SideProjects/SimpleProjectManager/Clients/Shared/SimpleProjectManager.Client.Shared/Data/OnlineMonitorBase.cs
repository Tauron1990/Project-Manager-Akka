using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Shared.ServerApi.RestApi;

namespace SimpleProjectManager.Client.Shared.Data;

public abstract class OnlineMonitorBase<TThis> : IOnlineMonitor
    where TThis : OnlineMonitorBase<TThis>
{
    protected readonly ILogger<TThis> Logger;
    private readonly IPingServiceDef _pingService;
    
    public IObservable<bool> Online { get; }

    // ReSharper disable once ContextualLoggerProblem
    protected OnlineMonitorBase(HttpClient client, ILogger<TThis> logger)
    {
        Logger = logger;
        _pingService = RestEase.RestClient.For<IPingServiceDef>(client);
        Online =
            Observable.Interval(TimeSpan.FromSeconds(6))
               .SelectMany(_ => IsOnline())
               .StartWith(true)
               .DistinctUntilChanged();
    }

    protected abstract ValueTask<bool> RunInternal(CancellationToken token);

    public async Task<bool> IsOnline()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            return await RunInternal(source.Token) && await _pingService.Ping(source.Token) == "ok";
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error on Ping Server");
            return false;
        }
    }
}