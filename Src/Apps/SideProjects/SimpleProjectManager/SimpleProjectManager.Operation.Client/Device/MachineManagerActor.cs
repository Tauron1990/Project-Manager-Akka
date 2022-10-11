using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Config;
using Tauron.TAkka;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed partial class MachineManagerActor : ReceiveActor
{
    private readonly IMachine _machine;
    private readonly ILogger<MachineManagerActor> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _deviceName;
    private readonly string _clientName;

    private DeviceInformations _device = DeviceInformations.Empty;
    private IActorRef _serverManager = ActorRefs.Nobody;
    
    public MachineManagerActor(IMachine machine, ILogger<MachineManagerActor> logger, OperationConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _deviceName = configuration.MachineInterface;
        _clientName = configuration.Name;
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

        while (stack.Count != 0)
        {
            var ele = stack.Pop();
            
            ele.Groups.ForEach(stack.Push);
            
            ele.Sensors.ForEach(
                sen => Context.ActorOf(
                    () => new MachineSensorActor(_deviceName, _machine, sen, _device.DeviceManager, _loggerFactory.CreateLogger<MachineSensorActor>()),
                    sen.Identifer));

            ele.DeviceButtons.ForEach(
                btn => Context.ActorOf(
                    () => new MachineButtonHandlerActor(_deviceName, _machine, btn, _device.DeviceManager),
                    btn.Identifer));
        }

        Receive<ButtonClick>(c => Context.Child(c.Identifer).Forward(c));
    }

    [LoggerMessage(EventId = 66, Level = LogLevel.Error, Message = "Error on Registrating Device with {error}")]
    private partial void ErrorOnRegisterDeviceOnServer(string error);
    
    private void Starting()
    {
        void Invalid() { }

        Receive<DeviceInfoResponse>(
            response =>
            {
                if(string.IsNullOrWhiteSpace(response.Error))
                {
                    Become(Running);
                    return;
                }
                
                ErrorOnRegisterDeviceOnServer(response.Error);
                Become(Invalid);
            });
    }

    [LoggerMessage(EventId = 65, Level = LogLevel.Error, Message = "Error on Collect Device Informations {deviceName}")]
    private partial void ErrorOnCollectDeviceinformations(Exception ex, string deviceName);

    protected override void PreStart()
    {
        _serverManager = Context.ActorOf(
            ClusterSingletonProxy
               .Props(DeviceInformations.ManagerPath, ClusterSingletonProxySettings.Create(Context.System)),
            "ServerConnection");

        Task.Run(async () =>
                 {
                     var data = await _machine.CollectInfo();

                     return data with { DeviceName = $"{_clientName}--{data.DeviceName}", DeviceManager = Self };
                 })
        .PipeTo(
                _serverManager,
                Self,
                failure: ex =>

                         {
                             ErrorOnCollectDeviceinformations(ex, _deviceName);

                             return PoisonPill.Instance;
                         },
                success: data =>
                         {
                             Interlocked.Exchange(ref _device, data);

                             return data;
                         });}
}
