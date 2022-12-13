using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DeviceState> _logger;

    public DeviceState(IStateFactory stateFactory, IDeviceService deviceService, ILogger<DeviceState> logger) 
        : base(stateFactory)
    {
        _stateFactory = stateFactory;
        _deviceService = deviceService;
        _logger = logger;
    }

    public IObservable<ImmutableDictionary<DeviceId, DeviceName>> CurrentDevices { get; private set; }
        = Observable.Empty<ImmutableDictionary<DeviceId, DeviceName>>();
    
    protected override IStateConfiguration<DeviceData> ConfigurateState(ISourceConfiguration<DeviceData> configuration)
        => configuration.FromServer(async t => new DeviceData(await _deviceService.GetAllDevices(t).ConfigureAwait(false)));


    protected override void PostConfiguration(IRootStoreState<DeviceData> state)
    {
        CurrentDevices = state.Select(dd => dd.Devices).Replay(1);
        
        base.PostConfiguration(state);
    }

    public IState<(DeviceUiGroup? UI, DeviceId? Id)> GetUiFetcher(Func<CancellationToken, ValueTask<DeviceId?>> idGetter)
    {
        async Task<(DeviceUiGroup?, DeviceId?)> GetUi(IComputedState<(DeviceUiGroup?, DeviceId?)> g, CancellationToken token)
        {
            DeviceId? id = await idGetter(token).ConfigureAwait(false);

            #if DEBUG
            if(id is null)
                _logger.LogWarning("No Device Id Found");
            #endif
            
            return id is null ? default : (await _deviceService.GetRootUi(id, token).ConfigureAwait(false), id);


        }

        return _stateFactory.NewComputed<(DeviceUiGroup?, DeviceId?)>(GetUi);
    }
}