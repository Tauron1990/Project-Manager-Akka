using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NLog;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using Stl.Async;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Server.AppCore.Helper
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public class ClusterConnectionTracker : IClusterConnectionTracker
    {
        private readonly IAppIpManager _manager;
        private readonly ILogger<ClusterConnectionTracker> _log;
        private          bool                              _isConnected;
        private          string                            _url = string.Empty;
        private readonly bool                              _isSelf;
        

        public ClusterConnectionTracker(ActorSystem system, IAppIpManager manager, ILogger<ClusterConnectionTracker> log)
        {
            _manager = manager;
            _log = log;
            var cluster = Cluster.Get(system);

            cluster.RegisterOnMemberUp(() =>
                                       {
                                           _isConnected = true;
                                           _url = cluster.SelfAddress.ToString();
                                           using (Computed.Invalidate())
                                           {
                                               GetIsConnected().Ignore();
                                               GetUrl().Ignore();
                                           }
                                       });
            cluster.RegisterOnMemberRemoved(() =>
                                            {
                                                _isConnected = false;
                                                using(Computed.Invalidate())
                                                    GetIsConnected().Ignore();
                                            });

            if (cluster.Settings.SeedNodes.Count != 0) return;
            _isSelf = true;

            RunInit();
            
            async void RunInit()
            {
                try
                {
                    var ip = await manager.GetIp();
                    if(cluster.Settings.SeedNodes.Count != 0) return;
                    if(!ip.IsValid) return;

                    await cluster.JoinAsync(cluster.SelfAddress);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error on Join Self Cluster");
                }
            }
        }

        public virtual Task<string> GetUrl()
            => Task.FromResult(_url);

        public virtual Task<bool> GetIsConnected()
            => Task.FromResult(_isConnected);

        public virtual Task<bool> GetIsSelf()
            => Task.FromResult(_isSelf);

        public virtual Task<AppIp> Ip()
            => _manager.GetIp();

        public virtual async Task<string?> ConnectToCluster(ConnectToClusterCommand command, CancellationToken token = default)
        {
            try
            {
                string content = await File.ReadAllTextAsync(Path.Combine(Program.ExeFolder, AkkaConfigurationBuilder.Seed));
                content = await AkkaConfigurationBuilder.PatchSeedUrls(content, new[] { command.Url });
                await File.WriteAllTextAsync(Path.Combine(Program.ExeFolder, AkkaConfigurationBuilder.Seed), content);
                return string.Empty;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Connect to Cluster");
                return e.Message;
            }
        }
    }
}