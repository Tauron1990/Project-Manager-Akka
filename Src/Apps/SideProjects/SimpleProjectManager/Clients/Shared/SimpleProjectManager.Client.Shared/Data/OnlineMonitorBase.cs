using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
    private readonly Lazy<IObservable<bool>> _onlineBuilder;

    public IObservable<bool> Online => _onlineBuilder.Value;

    // ReSharper disable once ContextualLoggerProblem
    protected OnlineMonitorBase(HttpClient client, ILogger<TThis> logger)
    {
        Logger = logger;
        _pingService = RestEase.RestClient.For<IPingServiceDef>(client);
        _onlineBuilder = new Lazy<IObservable<bool>>(
            () =>
                IsOnline().ToObservable()
                   .Concat
                    (
                        Observable.Interval(TimeSpan.FromSeconds(6))
                           .SelectMany(_ => IsOnline())
                    )
                   .DistinctUntilChanged()
                   .Replay(1).RefCount());
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
            if(e is not HttpRequestException)
                Logger.LogError(e, "Error on Ping Server");
            return false;
        }
    }
}