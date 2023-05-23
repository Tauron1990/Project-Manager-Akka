using System.Reactive.Disposables;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.UiHelper;

[PublicAPI]
public abstract partial class ScreenModelBase : IScreenModel
{
    private static readonly DeviceUiGroup Empty = DeviceUi.Text("Empty");

    private readonly Dictionary<DeviceId, Func<string, Task<SimpleResult>>> _inputHandlers = new();
    private readonly Dictionary<DeviceId, Func<DeviceSensor, Task<DeviceManagerMessages.ISensorBox>>> _sensorHandlers = new();
    private readonly Dictionary<DeviceId, ButtonRegistraction> _buttons = new();

    private readonly ILogger _logger;
    
    private DeviceUiGroup _current = Empty;
    private Action<DeviceUiGroup> _updateUi = static _ => { };
    private Action<string> _navigate = static _ => { };

    protected ScreenModelBase(ILogger logger) => _logger = logger;

    public DeviceUiGroup Initialize()
    {
        InitModel();
        DeviceUiGroup inter = CreateInitialUI();
        _current = inter;
        return inter;
    }

    protected abstract void InitModel();

    protected abstract DeviceUiGroup CreateInitialUI();
    
    public virtual void OnShow(UiManagerHelper helper)
    {
        _updateUi = helper.Replace;
        _navigate = helper.Show;
    }

    public virtual void OnHide() => _updateUi = static _ => { };

    [LoggerMessage(1, LogLevel.Warning, "No Sensor Registraion Found for {sensor}")]
    private partial void SensorNotFound(DeviceSensor sensor);
    
    public virtual Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor) => 
        _sensorHandlers.TryGetValue(sensor.Identifer, out var handller) 
            ? handller(sensor) 
            : Task.FromResult(DeviceManagerMessages.SensorBox.CreateDefault(sensor.SensorType));

    public virtual void ButtonClick(DeviceId identifer)
    {
        if(_buttons.TryGetValue(identifer, out ButtonRegistraction? registraction))
            registraction.OnClick();
    }

    public virtual void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
    {
        if(_buttons.TryGetValue(identifer, out ButtonRegistraction? registraction))
            registraction.ExtetnalSub.Disposable = registraction.CanClick.Subscribe(onButtonStateChanged, _ => onButtonStateChanged(obj: false));
    }

    public virtual async Task<SimpleResult> NewInput(DeviceId element, string input)
    {
        try
        {
            if(_inputHandlers.TryGetValue(element, out var handler))
                return await handler(input).ConfigureAwait(false);

            return SimpleResult.Failure("NewInput not Implemented");
        }
        catch (Exception e)
        {
            return SimpleResult.Failure(e);
        }
    }

    protected void NavigateTo(string name)
        => _navigate(name);
    
    protected void UpdateUi(Func<DeviceUiGroup, DeviceUiGroup> updater)
    {
        _current = updater(_current);
        _updateUi(_current);
    }

    protected void HandleInput(DeviceId id, Func<string, SimpleResult> handler) =>
        _inputHandlers.Add(id, s => Task.FromResult(handler(s)));
    
    protected void HandleInput(DeviceId id, Func<string, Task<SimpleResult>> handler) =>
        _inputHandlers.Add(id, handler);

    protected void HandleSensor(DeviceId id, Func<DeviceSensor, Task<DeviceManagerMessages.ISensorBox>> handler) =>
        _sensorHandlers.Add(id, handler);
    
    protected void HandleButton(DeviceId id, IObservable<bool> canClick, Action onClick)
    {
        var canclickBehavior = new BehaviorSubject<bool>(value: false);
        _buttons.Add(id, new ButtonRegistraction(canclickBehavior, onClick, canClick.AutoSubscribe(canclickBehavior), new SerialDisposable()));
    }

    private sealed record ButtonRegistraction(BehaviorSubject<bool> CanClick, Action OnClick, IDisposable Subscription, SerialDisposable ExtetnalSub);
}