using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using SimpleProjectManager.Operation.Client.Device.MGI.MgiUi.Screens;
using SimpleProjectManager.Operation.Client.Device.UiHelper;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi;

public sealed partial class MgiUiManager
{
    private readonly ILogger<MgiUiManager> _logger;

    private readonly IMutableState<string> _currentStatus;
    private readonly UiManagerHelper _screenManager;
    
    public MgiUiManager(ILoggerFactory loggerFactory, ITauronEnviroment tauronEnviroment, IStateFactory stateFactory)
    {
        _logger = loggerFactory.CreateLogger<MgiUiManager>();
        var uiConfiguration = new UiConfiguration(tauronEnviroment, loggerFactory.CreateLogger<UiConfiguration>());
        _currentStatus = stateFactory.NewMutable("Initialisirung");
        
        _screenManager = ScreenTypes.Init(
            new UiManagerHelper(stateFactory, DeviceUi.Text("Initialisirung")),
            uiConfiguration);

        _screenManager.Show(
            string.IsNullOrWhiteSpace(uiConfiguration.UvLampIp)
                ? ScreenTypes.Configuration
                : ScreenTypes.MainControl);
    }

    [LoggerMessage(1, LogLevel.Error, "Error on Process Logentry for Status Change")]
    private partial void ErrorOnprocessStatus(Exception e);

    public void ProcessStatus(LogInfo log)
    {
        try
        {
            if(log.Content.Contains("Pilot status switching", StringComparison.Ordinal))
            {
                string status = log.Content.Replace("Pilot status switching", string.Empty, StringComparison.Ordinal);
                _currentStatus.Set(status);
            }
            else if(string.Equals(log.Type, "ERROR", StringComparison.Ordinal))
                _currentStatus.Set($"Fehler: {log.Content}");
        }
        catch (Exception e)
        {
            ErrorOnprocessStatus(e);
        }
    }
    
    public DeviceUiGroup CreateUi() => _screenManager.UI.Value;

    public IState<DeviceUiGroup> DynamicUi => _screenManager.UI;
    
    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor) => 
        _screenManager.UpdateSensorValue(sensor);

    public void ButtonClick(DeviceId identifer) => 
        _screenManager.ButtonClick(identifer);

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged) =>
        _screenManager.WhenButtonStateChanged(identifer, onButtonStateChanged);

    public Task<SimpleResult> NewInput(DeviceId element, string input) =>
        _screenManager.NewInput(element, input);
}