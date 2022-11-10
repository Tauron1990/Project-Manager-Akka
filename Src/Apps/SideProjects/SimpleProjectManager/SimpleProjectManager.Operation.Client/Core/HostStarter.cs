using Akka.Actor;
using Akka.Cluster;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Operation.Client.Config;
using Tauron.Features;

namespace SimpleProjectManager.Operation.Client.Core;

public partial class HostStarter : BackgroundService
{
    private readonly OperationConfiguration _configuration;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<HostStarter> _logger;
    private readonly TaskCompletionSource _nameRegistrar = new();
    private readonly ActorSystem _system;

    public HostStarter(
        IHostApplicationLifetime hostApplicationLifetime, ILogger<HostStarter> logger, ActorSystem system,
        OperationConfiguration configuration)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _system = system;
        _configuration = configuration;

    }

    public Task NameRegistrated => _nameRegistrar.Task;

    [LoggerMessage(EventId = 56, Level = LogLevel.Critical, Message = "Error on Start Up Operations Client")]
    private partial void FatalErrorWhileStartHost(Exception e);

    [LoggerMessage(EventId = 57, Level = LogLevel.Critical, Message = "Name Registrating Failed, Shutdown.")]
    private partial void NameRegistrationFailed();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Cluster.Get(_system).JoinAsync(Address.Parse(_configuration.AkkaUrl), stoppingToken);
            IActorRef nameActor = _system.ActorOf("ClientNameManager", NameFeature.Create(_configuration.Name));
            var result = await NameRequest.Ask(nameActor, _logger);

            if(result.IsInValid())
            {
                NameRegistrationFailed();
                _nameRegistrar.TrySetException(new InvalidOperationException("Name Requestation Failed"));

                _hostApplicationLifetime.StopApplication();

                return;
            }

            _nameRegistrar.TrySetResult();
        }
        catch (Exception e)
        {
            if(e is OperationCanceledException)
                _nameRegistrar.TrySetCanceled(stoppingToken);
            else
                _nameRegistrar.TrySetException(e);

            FatalErrorWhileStartHost(e);
            _hostApplicationLifetime.StopApplication();
            await _system.Terminate();
        }
    }
}