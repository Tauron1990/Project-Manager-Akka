using Akka.Actor;
using Akka.Cluster;
using NLog;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;

namespace ServiceManager.Server.AppCore.Helper
{
    public sealed class ClusterConnectionTracker : ObservableObject, IClusterConnectionTracker
    {
        private readonly IPropertyChangedNotifer _notifer;
        private bool _isConnected;
        private string _url = string.Empty;

        public string Url
        {
            get => _url;
            private set => SetProperty(ref _url, value, () => _notifer.SendPropertyChanged<IClusterConnectionTracker>(nameof(Url)));
        }

        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value, () => _notifer.SendPropertyChanged<IClusterConnectionTracker>(nameof(IsConnected)));
        }

        public bool IsSelf { get; }

        public AppIp Ip { get; }

        public ClusterConnectionTracker(ActorSystem system, IAppIpManager manager, IPropertyChangedNotifer notifer)
        {
            _notifer = notifer;
            Ip = manager.Ip;
            var cluster = Cluster.Get(system);

            cluster.RegisterOnMemberUp(() =>
                                       {
                                           IsConnected = true;
                                           Url = cluster.SelfAddress.ToString();
                                       });
            cluster.RegisterOnMemberRemoved(() => IsConnected = false);

            if (cluster.Settings.SeedNodes.Count != 0) return;
            IsSelf = true;

            if(manager.Ip.IsValid)
                cluster.JoinAsync(cluster.SelfAddress)
                       .ContinueWith(t =>
                                     {
                                         if(t.IsFaulted)
                                            LogManager.GetCurrentClassLogger().Error(t.Exception, "Error on Join Self Cluster");
                                     });
        }
    }
}