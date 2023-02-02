using System.Reactive.Disposables;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Device.Core;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed partial class UIManagerActor : ReceiveActor
{
    private readonly IActorRef _serverManager;
    private readonly IMachine _machine;
    private readonly ILogger<UIManagerActor> _logger;
    private readonly DeviceId _deviceId;
    private IDisposable _subscription = Disposable.Empty;
    
    public UIManagerActor(IActorRef serverManager, IMachine machine, ILogger<UIManagerActor> logger, DeviceId deviceId)
    {
        _serverManager = serverManager;
        _machine = machine;
        _logger = logger;
        _deviceId = deviceId;

        Receive<Status.Failure>(OnFailure);
        Receive<DeviceUiGroup>(OnNewUiGroup);
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });
    }

    private void OnNewUiGroup(DeviceUiGroup uiGroup)
        => _serverManager.Tell(new DeviceManagerMessages.NewUIData(_deviceId, uiGroup));

    [LoggerMessage(68, LogLevel.Error, "Error on Compute UI State", EventName = "ComputeUIState")]
    private partial void ErrorOnComputeUIState(Exception cause);
    
    private void OnFailure(Status.Failure obj)
        => throw obj.Cause;

    protected override void PostStop()
        => _subscription.Dispose();

    protected override void PreStart()
    {
        var state = _machine.UIUpdates();
        if(state is null)
        {
            Context.Stop(Self);
            return;
        }

        IActorRef? self = Self;
        
        _subscription = state
           .ToObservable(ErrorOnComputeUIState)
           .Subscribe(u => self.Tell(u), ex => self.Tell(new Status.Failure(ex)));
    }
}