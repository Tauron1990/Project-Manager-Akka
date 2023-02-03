using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Logger.NLog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;
using SimpleProjectManager.Operation.Client.Setup.Devices;

namespace SimpleProjectManager.Operation.Client.Device;

public partial class DeviceStartUp
{
    private readonly ActorSystem _actorSystem;
    private readonly OperationConfiguration _configuration;
    private readonly HostStarter _hostStarter;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<DeviceStartUp> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DeviceStartUp(
        HostStarter hostStarter, OperationConfiguration configuration, ILogger<DeviceStartUp> logger,
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
    private partial void ErrorOnStartupDevice(Exception ex, in InterfaceId name);

    [LoggerMessage(EventId = 65, Level = LogLevel.Error, Message = "Device {name} not Created")]
    private partial void NoDeviceCreated(in InterfaceId name);

    public void Run()
    {
        try
        {
            if(!_configuration.Device.Active) return;

            IMachine? inter = MachineInterfaces.Create(_configuration.Device.MachineInterface, _serviceProvider);
            if(inter is null)
            {
                NoDeviceCreated(_configuration.Device.MachineInterface);

                return;
            }

            _actorSystem.ActorOf(
                DependencyResolver.For(_actorSystem)
                   .Props<DeviceSuperviser>(inter),
                "ClientDeviceManager");
        }
        catch (Exception e)
        {
            ErrorOnStartupDevice(e, _configuration.Device.MachineInterface);
            _lifetime.StopApplication();
        }
    }
}