using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;

public sealed class ConfigurationScreen : ScreenModelBase
{
    private readonly DeviceId _uvIpInput = DeviceId.ForName("UVLampIp");

    private readonly UiConfiguration _uiConfiguration;

    private ConfigurationScreen(UiConfiguration uiConfiguration) => _uiConfiguration = uiConfiguration;

    public static IScreenModel Create(UiConfiguration configuration)
        => new ConfigurationScreen(configuration);

    protected override DeviceUiGroup CreateInitialUI() =>
        DeviceUi.GroupVertical(
            "konfiguration",
            DeviceUi.TextInput(_uvIpInput, "Ip Uv Lampe", _uiConfiguration.UvLampIp));
}