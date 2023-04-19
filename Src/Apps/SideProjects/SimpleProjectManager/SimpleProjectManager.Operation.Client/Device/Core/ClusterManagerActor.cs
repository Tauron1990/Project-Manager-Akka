using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using Tauron.Features;

#pragma warning disable EPS02
#pragma warning disable SYSLIB1006

namespace SimpleProjectManager.Operation.Client.Device.Core;

public sealed partial class ClusterManagerActor 
    : ActorFeatureBase<(
        DistributedData Replicator, 
        ORDictionaryKey<string, DeviceInfoEntry> DictionaryKey,
        Cluster Cluseter
        )>
{
    public static IPreparedFeature New()
        => Feature.Create(
            () => new ClusterManagerActor(),
            c =>
            (
                c.System.DistributedData(),
                new ORDictionaryKey<string, DeviceInfoEntry>(DeviceManagerMessages.DeviceDataId),
                Cluster.Get(c.System)
            ));

    private ClusterManagerActor()
    {
    }
    
    protected override void ConfigImpl()
    {
        Receive<DeviceInformations>(NewDeviceData);
        Receive<Status.Failure>(OnError);
        
        Observ<DeviceServerOffline>().Subscribe();
        Observ<DeviceServerOnline>().Subscribe();
    }

    private void OnError(Status.Failure fail)
    {
        FatalErrorOnUpdate(Logger, fail.Cause);
        Context.Stop(Context.Self);
    }

    private void NewDeviceData(DeviceInformations obj)
    {
        RunUpdate(obj, Self)
           .PipeTo(Self)
           .Ignore();
    }

    private async Task RunUpdate(DeviceInformations info, IActorRef client)
    {
        (DistributedData replicator, var dictionaryKey, Cluster cluster) = CurrentState;
        try
        {

            UpdateDeviceInfo(Logger, info.DeviceId, info.Name);

            var dic = await replicator.GetAsync(dictionaryKey).ConfigureAwait(false)
                   ?? ORDictionary<string, DeviceInfoEntry>.Empty;

            var newEntry = DeviceInfoEntry.Create(client, info);

            dic = dic.AddOrUpdate(
                cluster,
                info.DeviceId.Value,
                newEntry,
                entry => entry.With(info));

            await replicator.UpdateAsync(dictionaryKey, dic, new WriteTo(2, TimeSpan.FromSeconds(20))).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            if(exception is not TimeoutException)
                throw;

            ReplicatorUpdateTimeout(Logger, info.DeviceId, info.Name);
            #pragma warning disable ERP022
        }
        #pragma warning restore ERP022
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Timeout while Updating {name} with id {id}")]
    private static partial void ReplicatorUpdateTimeout(ILogger logger, DeviceId id, in DeviceName name);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "New Data for Device {name} with id {id}")]
    private static partial void UpdateDeviceInfo(ILogger logger, DeviceId id, in DeviceName name);
    
    [LoggerMessage(Level = LogLevel.Critical, Message = "Critical error on Update Device Data")]
    private static partial void FatalErrorOnUpdate(ILogger logger, Exception ex);
}