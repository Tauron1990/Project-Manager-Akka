using System.Reactive;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.Devices;

public class SingleButtonViewModel : BlazorViewModel
{
    private readonly IState<DeviceButton?> _button;
    private readonly IMutableState<bool> _canClick;
    private readonly IState<DeviceId?> _deviceId;
    private readonly IDeviceService _deviceService;
    private readonly IEventAggregator _eventAggregator;
    private readonly IOnlineMonitor _onlineMonitor;

    public SingleButtonViewModel(IStateFactory stateFactory, IDeviceService deviceService, IEventAggregator eventAggregator, IOnlineMonitor onlineMonitor)
        : base(stateFactory)
    {
        _deviceService = deviceService;
        _eventAggregator = eventAggregator;
        _onlineMonitor = onlineMonitor;
        _canClick = stateFactory.NewMutable<bool>(initialOutput: true);

        _deviceId = GetParameter<DeviceId?>(nameof(SingleButtonDisplay.DeviceId));
        _button = GetParameter<DeviceButton?>(nameof(SingleButtonDisplay.Button));

        this.WhenActivated(Init);
    }

    public ReactiveCommand<Unit, Unit>? ButtonClick { get; private set; }

    private IEnumerable<IDisposable> Init()
    {
        IState<bool> canClickServer = StateFactory.NewComputed(new ComputedState<bool>.Options(), GetCanClick);

        yield return canClickServer.ToObservable(StateError).Subscribe(v => _canClick.Set(v));

        yield return ButtonClick = ReactiveCommand.CreateFromTask(
            ClickButton,
            _canClick
               .ToObservable(StateError)
               .AndIsOnline(_onlineMonitor));
    }

    private bool StateError(Exception ex)
    {
        _eventAggregator.PublishError(ex);

        return false;
    }

    private async Task ClickButton(CancellationToken token)
    {
        _canClick.Set(false);

        DeviceId? deviceId = _deviceId.ValueOrDefault;
        DeviceButton? button = _button.ValueOrDefault;

        if(deviceId is null || button is null)
        {
            _eventAggregator.PublishWarnig("Die Ids für den Button wurden nicht gefunden");
            return;
        }

        await _eventAggregator.IsSuccess(
                async () => await TimeoutToken.WithDefault(
                        token,
                        async t => await _deviceService.ClickButton(deviceId, button.Identifer, t).ConfigureAwait(false))
                   .ConfigureAwait(false))
           .ConfigureAwait(false);
    }

    private async Task<bool> GetCanClick(IComputedState<bool> state, CancellationToken token)
    {
        DeviceId? deviceId = await _deviceId.Use(token).ConfigureAwait(false);
        DeviceButton? button = await _button.Use(token).ConfigureAwait(false);

        if(deviceId is null || button is null) return false;

        return await _deviceService.CanClickButton(deviceId, button.Identifer, token).ConfigureAwait(false);

    }
}