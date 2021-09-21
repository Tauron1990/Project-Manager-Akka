using System;
using System.Threading.Tasks;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConnectToClusterViewModel
    {
        private readonly IEventAggregator _aggregator;
        private readonly IServerInfo _serverInfo;
        private readonly IClusterConnectionTracker _tracker;

        public ConnectToClusterViewModel(IServerInfo serverInfo, IEventAggregator aggregator, IClusterConnectionTracker tracker)
        {
            _serverInfo = serverInfo;
            _aggregator = aggregator;
            _tracker = tracker;
        }

        public Func<string, string?> ValidateUrl => s =>
                                                    {
                                                        if (string.IsNullOrWhiteSpace(s))
                                                            return "Bitte Url Angeben";
                                                        if (!Uri.TryCreate(s, UriKind.Absolute, out var result))
                                                            return "Das ist Keine Korrekte Uri";

                                                        if (result.Scheme != "akka.tcp")
                                                            return "Keine akka tcp Uri Schema: akka.tcp://";
                                                        if (result.UserInfo != "Project-Manager")
                                                            return "Der name des Cluster mus \"Project-Manager\" lauten";

                                                        return null;
                                                    };

        public string ClusterUrl { get; set; } = string.Empty;

        public IOperationManager Operation { get; } = new OperationManager();

        public async Task ConnectToCluster()
        {
            using (Operation.Start())
            {
                try
                {
                    if (ValidateUrl(ClusterUrl) != null) return;

                    var result = await _tracker.ConnectToCluster(new ConnectToClusterCommand(ClusterUrl));

                    if (result != string.Empty) return;

                    await _serverInfo.Restart(new RestartCommand());
                }
                catch (Exception e)
                {
                    _aggregator.PublishError(e);
                }
            }
        }
    }
}