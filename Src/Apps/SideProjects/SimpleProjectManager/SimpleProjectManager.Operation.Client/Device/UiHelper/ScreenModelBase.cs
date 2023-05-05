using System.Reactive.Subjects;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.UiHelper;

public abstract class ScreenModelBase : IScreenModel
{
    private static readonly DeviceUiGroup Empty = DeviceUi.Text("Empty");

    private readonly Dictionary<DeviceId, Func<SimpleResult, string>> _inputHandlers = new();
    private readonly Dictionary<DeviceId, Func<DeviceSensor, Task<DeviceManagerMessages.ISensorBox>>> _sensorHandlers = new();
    private readonly Dictionary<DeviceId, ButtonRegistraction> _buttons = new();

    private DeviceUiGroup _current = Empty;
    private Action<DeviceUiGroup> _updateUi = static _ => { };

    public DeviceUiGroup Initialize()
    {
        InitModel();
        DeviceUiGroup inter = CreateInitialUI();
        _current = inter;
        return inter;
    }

    protected virtual void InitModel(){}

    protected abstract DeviceUiGroup CreateInitialUI();
    
    public virtual void OnShow(UiManagerHelper helper) => _updateUi = helper.Replace;

    public virtual void OnHide() => _updateUi = static _ => { };

    public virtual Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor) =>
        Task.FromResult(DeviceManagerMessages.SensorBox.CreateDefault(sensor.SensorType));

    public virtual void ButtonClick(DeviceId identifer) { }

    public virtual void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged) { }

    public virtual Task<SimpleResult> NewInput(DeviceId element, string input) =>
        Task.FromResult(SimpleResult.Failure("NewInput not Implemented"));

    public void UpdateUi(Func<DeviceUiGroup, DeviceUiGroup> updater)
    {
        _current = updater(_current);
        _updateUi(_current);
    }

    
    
    private sealed record ButtonRegistraction(BehaviorSubject<bool> CanClick, Action OnClick, IDisposable Subscription);
}