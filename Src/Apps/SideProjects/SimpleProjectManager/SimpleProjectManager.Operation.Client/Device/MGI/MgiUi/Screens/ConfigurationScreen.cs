using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;

public sealed class ConfigurationScreen : ScreenModelBase
{
    private readonly DeviceId _uvIpInput = DeviceId.ForName("UVLampIp");

    private readonly UiConfiguration _uiConfiguration;

    private ConfigurationScreen(UiConfiguration uiConfiguration, ILogger<ConfigurationScreen> logger) 
        : base(logger) => _uiConfiguration = uiConfiguration;

    public static IScreenModel Create((UiConfiguration configuration, ILoggerFactory factory) parms)
        => new ConfigurationScreen(parms.configuration, parms.factory.CreateLogger<ConfigurationScreen>());

    protected override DeviceUiGroup CreateInitialUI() =>
        DeviceUi.GroupVertical(
            "konfiguration",
            DeviceUi.TextInput(_uvIpInput, "Ip Uv Lampe", _uiConfiguration.UvLampIp));

    public override async Task<SimpleResult> NewInput(DeviceId element, string input)
    {
        if(element == _uvIpInput)
        {
            _uiConfiguration.UvLampIp = input;
            return await SimpleResult.Catch(_uiConfiguration.RunSave()).ConfigureAwait(false);
        }
        
        return SimpleResult.Failure($"Kein Element in ConfigurationScreen bekannt ({element})");
    }
}