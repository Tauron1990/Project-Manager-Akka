using System;
using System.Collections.Generic;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using ReactiveUI;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public abstract class LogViewModel : ViewModelBase, IDisposable
{
    private readonly Func<IState<DeviceId?>> _device;
    private readonly SourceCache<LogData, DateTime> _logEntrys;
    private readonly IStateFactory _stateFactory;

    protected LogViewModel(IStateFactory stateFactory)
    {
        _stateFactory = stateFactory;
        _device = SimpleLazy.Create(stateFactory, GetDevice);
        _logEntrys = new SourceCache<LogData, DateTime>(e => e.Occurance);

        this.WhenActivated(Init);
    }

    public IObservableCollection<IGroup<LogData, DateTime, string>> Entrys { get; } = new ObservableCollectionExtended<IGroup<LogData, DateTime, string>>();

    public void Dispose()
        => _logEntrys.Dispose();

    [PublicAPI]
    protected virtual IEnumerable<IDisposable> Init()
    {


        yield return _logEntrys
           .Connect()
           .Group(ld => ld.Category.Value)
           .Bind(Entrys)
           .Subscribe();


    }

    protected abstract IState<DeviceId?> GetDevice(IStateFactory stateFactory);
}