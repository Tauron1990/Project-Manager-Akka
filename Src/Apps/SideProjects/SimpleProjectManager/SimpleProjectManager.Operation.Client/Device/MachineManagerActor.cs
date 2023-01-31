using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.DistributedData;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Device.Core;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using Tauron.Application;
using Tauron.Operations;
using Tauron.TAkka;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed partial class MachineManagerActor : ReceiveActor
{
    private readonly InterfaceId _deviceName;
    private readonly ILogger<MachineManagerActor> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMachine _machine;

    private DeviceInformations _device = DeviceInformations.Empty;
    private IActorRef _serverManager = ActorRefs.Nobody;

    public MachineManagerActor(IMachine machine, ILogger<MachineManagerActor> logger, OperationConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _deviceName = configuration.Device.MachineInterface;
        _machine = machine;
        _logger = logger;
        _loggerFactory = loggerFactory;

        Become(Starting);
    }

    protected override void PostStop()
    {
        if(_machine is IDisposable disposable)
            disposable.Dispose();
        base.PostStop();
    }

    private void Running()
    {
        var stack = new Stack<DeviceUiGroup>();
        stack.Push(_device.RootUi);

        _device.CollectButtons().Foreach(
            btn => Context.ActorOf(
                () => new MachineButtonHandlerActor(_device.DeviceId, _machine, btn.Button, _serverManager, btn.State),
                btn.Button.Identifer.Value));

        _device.CollectSensors().Foreach(
            sen => Context.ActorOf(
                () => new MachineSensorActor(_device.DeviceId, _machine, sen, _serverManager, _loggerFactory.CreateLogger<MachineSensorActor>()),
                sen.Identifer.Value));

        Context.ActorOf(() => new UIManagerActor(_serverManager, _machine, _loggerFactory.CreateLogger<UIManagerActor>(), _device.DeviceId), "UIManager");
        
        if(_device.HasLogs)
            Context.ActorOf(() => new LoggerActor(_machine, _device.DeviceId));

        Receive<ButtonClick>(c => Context.Child(c.Identifer.Value).Forward(c));
    }

    [LoggerMessage(EventId = 66, Level = LogLevel.Error, Message = "Error on Registrating Device with {error}")]
    #pragma warning disable EPS05
    private partial void ErrorOnRegisterDeviceOnServer(SimpleResult error);
    #pragma warning restore EPS05

    private void Starting()
    {
        void Invalid() { }

        Receive<DeviceInfoResponse>(
            response =>
            {
                if(response.Result.IsSuccess())
                {
                    Become(Running);

                    return;
                }

                ErrorOnRegisterDeviceOnServer(response.Result);
                Become(Invalid);
            });
    }

    [LoggerMessage(EventId = 65, Level = LogLevel.Error, Message = "Error on Collect Device Informations {deviceName}")]
    private partial void ErrorOnCollectDeviceinformations(Exception ex, in InterfaceId deviceName);

    protected override void PreStart()
    {
        _serverManager = Context.ActorOf(
            ClusterSingletonProxy
               .Props(DeviceInformations.ManagerPath, ClusterSingletonProxySettings.Create(Context.System)),
            "ServerConnection");

        
        Init(Context)
           .PipeTo(
                Context.ActorOf<ClusterManager>("ClusterManager"),
                Self,
                failure: ex =>
                         {
                             ErrorOnCollectDeviceinformations(ex, _deviceName);

                             return PoisonPill.Instance;
                         })
           .Ignore();
    }

    private async Task<DeviceInformations> Init(IActorContext context)
    {
        await _machine.Init(context).ConfigureAwait(false);

        DeviceInformations data = await _machine.CollectInfo().ConfigureAwait(false);
        _device = data with { DeviceManager = context.Self };

        return _device;
    }
}