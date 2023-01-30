using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public abstract class DeviceViewModel : ViewModelBase
{
    private readonly Func<IState<DevicePair?>> _deviceState;
    private readonly GlobalState _state;

    protected DeviceViewModel(GlobalState state, IStateFactory stateFactory)
    {
        _state = state;
        _deviceState = SimpleLazy.Create(stateFactory, CreateDeviceSelector);

        this.WhenActivated(Init);
    }

    public IState<(DeviceUiGroup? UI, DeviceId? Id)>? Ui { get; private set; }

    private IEnumerable<IDisposable> Init()
    {
        var state = _deviceState();

        Ui = _state.Devices.GetUiFetcher(
            async t =>
            {
                DevicePair? result = await state.Use(t).ConfigureAwait(false);

                #if DEBUG
                Console.WriteLine($"DeviceViewModel: NewPair: {result}");
                #endif

                return result?.Id;
            });

        yield return Disposable.Create(this, static self => self.Ui = null);
    }

    protected abstract IState<DevicePair?> CreateDeviceSelector(IStateFactory stateFactory);
}