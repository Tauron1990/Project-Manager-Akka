﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;


namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public sealed class DevicesViewModel : ViewModelBase
{
    private readonly GlobalState _state;
    private DevicePair _selectedPair;

    public IObservable<DevicePair[]> Devices { get; }

    public DevicePair SelectedPair
    {
        get => _selectedPair;
        set => this.RaiseAndSetIfChanged(ref _selectedPair, value);
    }

    public DevicesViewModel(GlobalState state)
    {
        _state = state;
        Devices = state.Devices.CurrentDevices.Select(dd => dd.Select(p => new DevicePair(p.Value, p.Key)).ToArray());
        
        this.WhenActivated(Init);
    }

    private IEnumerable<IDisposable> Init()
    {
        yield break;
    }
}