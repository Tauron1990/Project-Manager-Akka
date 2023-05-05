using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;

public sealed class ConfigurationScreen : IScreenModel
{
    private readonly DeviceId _uvIpInput = DeviceId.ForName("UVLampIp");
    
    private readonly UiConfiguration _uiConfiguration;

    private ConfigurationScreen(UiConfiguration uiConfiguration) => _uiConfiguration = uiConfiguration;

    public static IScreenModel Create(UiConfiguration configuration)
        => new ConfigurationScreen(configuration);

    public DeviceUiGroup Initialize() =>
        CreateUI();

     private DeviceUiGroup CreateUI() =>
        DeviceUi.GroupVertical("konfiguration",
            DeviceUi.TextInput(_uvIpInput, "Ip Uv Lampe", _uiConfiguration.UvLampIp));
    
    public void OnShow(UiManagerHelper helper)
    {
        throw new NotImplementedException();
    }

    public void OnHide()
    {
        throw new NotImplementedException();
    }

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor) => throw new NotImplementedException();

    public void ButtonClick(DeviceId identifer)
    {
        throw new NotImplementedException();
    }

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
    {
        throw new NotImplementedException();
    }

    public Task<SimpleResult> NewInput(DeviceId element, string input) => throw new NotImplementedException();
}