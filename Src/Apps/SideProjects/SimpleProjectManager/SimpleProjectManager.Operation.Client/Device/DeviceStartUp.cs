using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;

namespace SimpleProjectManager.Operation.Client.Device;

public class DeviceStartUp
{
    private readonly HostStarter _hostStarter;
    private readonly OperationConfiguration _configuration;
    private readonly ILogger<DeviceStartUp> _logger;

    public DeviceStartUp(HostStarter hostStarter, OperationConfiguration configuration, ILogger<DeviceStartUp> logger)
    {
        _hostStarter = hostStarter;
        _configuration = configuration;
        _logger = logger;
    }

    public async void Run()
    {
        try
        {
            if(!_configuration.Device) return;
            
            await _hostStarter.NameRegistrated;
            
            
        }
        catch (Exception e)
        {
            //later
        }
    }
}