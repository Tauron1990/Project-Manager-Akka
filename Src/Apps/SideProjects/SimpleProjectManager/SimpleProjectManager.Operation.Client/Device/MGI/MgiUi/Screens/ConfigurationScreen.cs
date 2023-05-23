using System.Net;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;

public sealed class ConfigurationScreen : ScreenModelBase
{
    private readonly DeviceId _uvIpInput = DeviceId.ForName("UVLampIp");

    private readonly UiConfiguration _uiConfiguration;

    private ConfigurationScreen(UiConfiguration uiConfiguration, ILogger logger) 
        : base(logger) => _uiConfiguration = uiConfiguration;

    public static IScreenModel Create((UiConfiguration configuration, ILoggerFactory factory) parm)
        => new ConfigurationScreen(parm.configuration, parm.factory.CreateLogger<ConfigurationScreen>());

    protected override void InitModel()
    {
        HandleInput(_uvIpInput, HandleNewIp);
    }

    protected override DeviceUiGroup CreateInitialUI() =>
        DeviceUi.GroupVertical(
            "konfiguration",
            DeviceUi.TextInput(_uvIpInput, "Ip Uv Lampe", _uiConfiguration.UvLampIp));

    private SimpleResult HandleNewIp(string input)
    {
        if(!IPAddress.TryParse(input, out _))
            return SimpleResult.Failure("Keine IP Adresse");
    
        _uiConfiguration.UvLampIp = input;
        
        NavigateTo(ScreenTypes.MainControl);
        
        return SimpleResult.Success();
    }
}