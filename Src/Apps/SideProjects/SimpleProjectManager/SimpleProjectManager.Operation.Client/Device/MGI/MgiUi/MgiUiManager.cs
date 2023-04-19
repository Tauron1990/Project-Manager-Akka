using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi;

public sealed partial class MgiUiManager
{
    private readonly ILogger<MgiUiManager> _logger;

    public MgiUiManager(ILogger<MgiUiManager> logger) =>
        _logger = logger;

    [LoggerMessage(1, LogLevel.Error, "Error on Process Logentry for Status Change")]
    private partial void ErrorOnprocessStatus(Exception e);

    public void ProcessStatus(LogInfo log)
    {
        try
        {
            
        }
        catch (Exception e)
        {
            ErrorOnprocessStatus(e);
        }
    }
    
    public DeviceUiGroup CreateUi()
    {
        return DeviceUi.GroupVertical(
            "MainPanel");
    }
}