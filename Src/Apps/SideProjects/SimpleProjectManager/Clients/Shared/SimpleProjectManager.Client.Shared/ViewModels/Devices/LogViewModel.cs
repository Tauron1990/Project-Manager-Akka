using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public abstract class LogViewModel : ViewModelBase, IDisposable
{
    private readonly Func<IState<DeviceId?>> _device;
    private readonly SourceCache<LogData, DateTime> _logEntrys;
    protected readonly IStateFactory StateFactory;
    private readonly IDeviceService _deviceService;

    protected LogViewModel(IStateFactory stateFactory, IDeviceService deviceService)
    {
        StateFactory = stateFactory;
        _deviceService = deviceService;
        _device = SimpleLazy.Create(stateFactory, GetDevice);
        _logEntrys = new SourceCache<LogData, DateTime>(e => e.Occurance);

        this.WhenActivated(Init);
    }

    public IObservableCollection<LogGroup> Entrys { get; } = new ObservableCollectionExtended<LogGroup>();

    #pragma warning disable CA1816
    public virtual void Dispose()
        #pragma warning restore CA1816
        => _logEntrys.Dispose();

    [PublicAPI]
    protected virtual IEnumerable<IDisposable> Init()
    {
        IState<DeviceId?> idDevice = _device();
        DateTime from = DateTime.Now;
        DeviceId? id = null;

        IState<DateTime> nextDate = StateFactory.NewComputed(new ComputedState<DateTime>.Options(), GetCurrent);

        yield return nextDate
           .ToObservable(ErrorOnFetchCurrentLogs)
           .SelectMany(
                async date =>
                {
                    if(id is null) return null;
                    
                    try
                    {
                        Logs result = await _deviceService.GetBatches(id, from, date, CancellationToken.None).ConfigureAwait(false);
                        from = date;
                        return result;
                    }
                    catch (Exception e)
                    {
                        ErrorOnFetchCurrentLogs(e);
                        return null;
                    }
                })
           .NotNull()
           .Subscribe(l => _logEntrys.AddOrUpdate(l.Data.SelectMany(b => b.Logs)));
        
        
        
        yield return _logEntrys
           .Connect()
           .Group(ld => ld.Category.Value)
           .Transform(g => new LogGroup(g))
           .DisposeMany()
           .Bind(Entrys)
           .Subscribe();
        
        async Task<DateTime> GetCurrent(IComputedState<DateTime> unused, CancellationToken token)
        {
            DeviceId? device = await idDevice.Use(token).ConfigureAwait(false);
            if(device is null) return DateTime.MinValue;
            
            if(Equals(device, id))
                return await _deviceService.CurrentLogs(device, token).ConfigureAwait(false);

            id = device;
            from = DateTime.MinValue;

            return await _deviceService.CurrentLogs(device, token).ConfigureAwait(false);
        }
    }

    protected abstract void ErrorOnFetchCurrentLogs(Exception exception);
    
    protected abstract IState<DeviceId?> GetDevice(IStateFactory stateFactory);
}