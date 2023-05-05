using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.DependencyInjection;
using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.UiHelper;

public class UiManagerHelper : IHasServices
{
    private readonly object _lock = new();
    private readonly Dictionary<string, ScreenRegistration> _screens = new(StringComparer.Ordinal);
    private readonly IMutableState<DeviceUiGroup> _currentui;
    
    private ScreenRegistration? _current;

    public IState<DeviceUiGroup> UI => _currentui;
    
    public IServiceProvider Services { get; }
    
    public UiManagerHelper(IStateFactory factory, DeviceUiGroup initialUi)
    {
        _currentui = factory.NewMutable(initialUi);
        Services = factory.Services;
    }

    public UiManagerHelper WithScreen(string key, SimpleLazy.Lazy<IScreenModel> model)
    {
        lock(_lock)
            _screens.Add(key, new ScreenRegistration(model));
        return this;
    }

    public void Replace(DeviceUiGroup group)
    {
        lock (_lock)
        {
            if(_current is not null)
                _current.Ui = group;
        }
        
        _currentui.Set(group);
    }

    public void Show(string name)
    {
        lock (_lock)
        {
            _current?.Model.OnHide();
            ScreenRegistration reg = _screens[name];
            _current = reg;
            _currentui.Set(reg.Ui);
            
            _current.Model.OnShow(this);
        }
    }

    public async Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor)
    {
        IScreenModel? model;
        lock (_lock)
            model = _current?.Model;

        if(model is null) return DeviceManagerMessages.SensorBox.CreateDefault(sensor.SensorType);

        return await model.UpdateSensorValue(sensor).ConfigureAwait(false);
    }

    public void ButtonClick(DeviceId identifer)
    {
        lock(_lock)
            _current?.Model.ButtonClick(identifer);
    }

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
    {
        lock(_lock)
            _current?.Model.WhenButtonStateChanged(identifer, onButtonStateChanged);
    }

    public async Task<SimpleResult> NewInput(DeviceId element, string input)
    {
        IScreenModel? model;
        lock (_lock)
            model = _current?.Model;

        if(model is null) return SimpleResult.Failure("No UI Screen Model Found");

        return await model.NewInput(element, input).ConfigureAwait(false);
    }

    private sealed class ScreenRegistration
    {
        private readonly SimpleLazy.Lazy<IScreenModel> _model;
        private DeviceUiGroup? _ui;

        internal DeviceUiGroup Ui
        {
            get => _ui ??= _model.Value.Initialize();
            set => _ui = value;
        }

        internal IScreenModel Model => _model.Value;
        
        internal ScreenRegistration(SimpleLazy.Lazy<IScreenModel> model) => _model = model;
    }
}