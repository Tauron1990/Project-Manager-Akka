using Akka.Actor;
using Akka.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;

namespace SimpleProjectManager.Operation.Client.Device;

public partial class DeviceStartUp
{
    private readonly HostStarter _hostStarter;
    private readonly OperationConfiguration _configuration;
    private readonly ILogger<DeviceStartUp> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly ActorSystem _actorSystem;

    public DeviceStartUp(HostStarter hostStarter, OperationConfiguration configuration, ILogger<DeviceStartUp> logger, 
        IHostApplicationLifetime lifetime, IServiceProvider serviceProvider, ActorSystem actorSystem)
    {
        _hostStarter = hostStarter;
        _configuration = configuration;
        _logger = logger;
        _lifetime = lifetime;
        _serviceProvider = serviceProvider;
        _actorSystem = actorSystem;
    }

    [LoggerMessage(EventId = 64, Level = LogLevel.Critical, Message = "Error on Start Up DeviceInterfece {name}")]
    private partial void ErrorOnStartupDevice(Exception ex, string name);

    [LoggerMessage(EventId = 65, Level = LogLevel.Error, Message = "Device {name} not Created")]
    private partial void NoDeviceCreated(string name);
    
    public async void Run()
    {
        try
        {
            if(!_configuration.Device) return;
            
            await _hostStarter.NameRegistrated;

            var inter = MachineInterfaces.Create(_configuration.MachineInterface, _serviceProvider);
            if(inter is null)
            {
                NoDeviceCreated(_configuration.MachineInterface);
                return;
            }

            await inter.Init();

            _actorSystem.ActorOf(
                DependencyResolver.For(_actorSystem)
                   .Props<MachineManagerActor>(inter));
        }
        catch (Exception e)
        {
            ErrorOnStartupDevice(e, _configuration.MachineInterface);
            _lifetime.StopApplication();
        }
    }
}