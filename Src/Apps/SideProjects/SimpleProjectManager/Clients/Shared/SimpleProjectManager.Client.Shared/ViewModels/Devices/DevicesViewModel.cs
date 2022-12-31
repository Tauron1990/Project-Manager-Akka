using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using Vogen;

namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public sealed class DevicesViewModel : ViewModelBase
{
    private DevicePair? _selectedPair;

    public DevicesViewModel(GlobalState state)
    {
        Devices = state.Devices.CurrentDevices.Select(dd => dd.Select(p => new DevicePair(p.Value, p.Key)).ToArray()); this.WhenActivated(Init);
    }

    public IObservable<DevicePair[]> Devices { get; }

    public DevicePair? SelectedPair
    {
        get => _selectedPair;
        set => this.RaiseAndSetIfChanged(ref _selectedPair, value);
    }

    private IEnumerable<IDisposable> Init()
    {
        yield return Devices.Subscribe(
            d =>
            {
                if(d.Length == 1)
                    SelectedPair = d[0];
            });

    }
}