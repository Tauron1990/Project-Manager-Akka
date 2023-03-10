using System.Reactive.Disposables;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Device.Core;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using Tauron.Features;
using Tauron.TAkka;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed partial class UiManagerActor : ActorFeatureBase<UiManagerActor.State>
{
    public sealed record State(IActorRef ServerManager, IMachine Machine, DeviceId DeviceId, IDisposable Subscription) : IDisposable
    {
        public void Dispose() => Subscription.Dispose();
    }

    public static IPreparedFeature New(IActorRef serverManager, IMachine machine, DeviceId deviceId)
        => Feature.Create(() => new UiManagerActor(), _ => new State(serverManager, machine, deviceId, Disposable.Empty));
    
    private readonly ILogger _logger;
    
    private UiManagerActor() => _logger = Logger;

    protected override void ConfigImpl()
    {
        Start.Subscribe(TryStart);

        ReceiveState<IDisposable>(sub => sub.State with { Subscription = sub.Event });
        
        Receive<Status.Failure>(OnFailure);
        Receive<DeviceUiGroup>(OnNewUiGroup);
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });
    }

    private void TryStart(IActorContext context)
    {
        var state = CurrentState.Machine.UIUpdates();
        if(state is null)
        {
            context.Stop(Self);
            return;
        }

        IActorRef self = Self;
        
        IDisposable subscription = state
            .ToObservable(ErrorOnComputeUIState)
            .Subscribe(u => self.Tell(u), ex => self.Tell(new Status.Failure(ex)));
        
        self.Tell(subscription);
    }
    
    private void OnNewUiGroup(DeviceUiGroup uiGroup)
        => CurrentState.ServerManager.Tell(new DeviceManagerMessages.NewUIData(CurrentState.DeviceId, uiGroup));

    [LoggerMessage(68, LogLevel.Error, "Error on Compute UI State", EventName = "ComputeUIState")]
    private partial void ErrorOnComputeUIState(Exception cause);
    
    private void OnFailure(Status.Failure obj)
        => throw obj.Cause;
}