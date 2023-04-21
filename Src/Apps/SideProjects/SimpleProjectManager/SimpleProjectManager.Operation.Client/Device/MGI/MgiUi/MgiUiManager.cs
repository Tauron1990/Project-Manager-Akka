using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi;

public sealed partial class MgiUiManager
{
    private readonly ILogger<MgiUiManager> _logger;
    private readonly UiConfiguration _uiConfiguration;
    
    private readonly IMutableState<string> _currentStatus;
    private readonly IMutableState<DeviceUiGroup> _deviceUi;

    public MgiUiManager(ILogger<MgiUiManager> logger, ITauronEnviroment tauronEnviroment, IStateFactory stateFactory)
    {
        _logger = logger;
        _uiConfiguration = new UiConfiguration(tauronEnviroment);
        _currentStatus = stateFactory.NewMutable(new MutableState<string>.Options { InitialValue = "Initialisirung" });
        _deviceUi = stateFactory.NewMutable(new MutableState<DeviceUiGroup>.Options { InitialValue = DeviceUi.Text("Initialisirung") });
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
    
    public DeviceUiGroup CreateUi() => _deviceUi.ValueOrDefault ?? DeviceUi.Text("Unbekannt");

    public IState<DeviceUiGroup> DynamicUi => _deviceUi;
    
    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
        => Task.FromResult(DeviceManagerMessages.SensorBox.CreateDefault(sensor.SensorType));

    public void ButtonClick(DeviceId identifer)
    {
        
    }

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
    {
        
    }

    public Task<SimpleResult> NewInput(DeviceId element, string input) => Task.FromResult(SimpleResult.Success());
}