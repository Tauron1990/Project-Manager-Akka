using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Shared.Data.States;

public sealed class DeviceState : StateBase<DeviceData>
{
    private readonly IStateFactory _stateFactory;
    private readonly IDeviceService _deviceService;

    public DeviceState(IStateFactory stateFactory, IDeviceService deviceService) 
        : base(stateFactory)
    {
        _stateFactory = stateFactory;
        _deviceService = deviceService;
    }

    public IObservable<ImmutableDictionary<DeviceId, DeviceName>> CurrentDevices { get; private set; }
        = Observable.Empty<ImmutableDictionary<DeviceId, DeviceName>>();
    
    protected override IStateConfiguration<DeviceData> ConfigurateState(ISourceConfiguration<DeviceData> configuration)
        => configuration.FromServer(async t => new DeviceData(await _deviceService.GetAllDevices(t).ConfigureAwait(false)));


    protected override void PostConfiguration(IRootStoreState<DeviceData> state)
    {
        CurrentDevices = state.Select(dd => dd.Devices);
        
        base.PostConfiguration(state);
    }

    public IState<DeviceUiGroup?> GetUiFetcher(Func<CancellationToken, ValueTask<DeviceId?>> idGetter)
    {
        async Task<DeviceUiGroup?> GetUi(IComputedState<DeviceUiGroup> g, CancellationToken token)
        {
            DeviceId? id = await idGetter(token).ConfigureAwait(false);

            if(id is null) return null;
            
            
            return await _deviceService.GetRootUi(id, token).ConfigureAwait(false);
        }

        return _stateFactory.NewComputed<DeviceUiGroup>(GetUi);
    }
}