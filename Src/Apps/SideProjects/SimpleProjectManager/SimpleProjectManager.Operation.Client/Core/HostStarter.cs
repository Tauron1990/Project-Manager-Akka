using Akka.Actor;
using Akka.Cluster;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Client.Operations.Shared.Clustering;
using SimpleProjectManager.Operation.Client.Config;
using Tauron.TAkka;

namespace SimpleProjectManager.Operation.Client.Core;

public partial class HostStarter : BackgroundService
{
    private readonly OperationConfiguration _configuration;
    private readonly NameService _nameService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<HostStarter> _logger;
    private readonly ActorSystem _system;

    public HostStarter(
        IHostApplicationLifetime hostApplicationLifetime, ILoggerFactory loggerFactory, ActorSystem system,
        OperationConfiguration configuration, NameService nameService)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<HostStarter>();
        _system = system;
        _configuration = configuration;
        _nameService = nameService;

    }

    [LoggerMessage(EventId = 56, Level = LogLevel.Critical, Message = "Error on Start Up Operations Client")]
    private partial void FatalErrorWhileStartHost(Exception e);

    [LoggerMessage(EventId = 57, Level = LogLevel.Critical, Message = "Name Registrating Failed, Shutdown.")]
    private partial void NameRegistrationFailed();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var logsProvider = _system.ActorOf(() => new ClusterLogProvider(_loggerFactory.CreateLogger<ClusterLogProvider>()));
            ClusteringApi.Get(_system).Register(logsProvider);
            
            await Cluster.Get(_system).JoinAsync(Address.Parse(_configuration.AkkaUrl), stoppingToken).ConfigureAwait(false);
            _system.ActorOf(() => new NameClient(_configuration.Name, _nameService), "ClientNameManager");
        }
        catch (Exception e)
        {
            FatalErrorWhileStartHost(e);
            _hostApplicationLifetime.StopApplication();
            await _system.Terminate().ConfigureAwait(false);
        }
    }
}