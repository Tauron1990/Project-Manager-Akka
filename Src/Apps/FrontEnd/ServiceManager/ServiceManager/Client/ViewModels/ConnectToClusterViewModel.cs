using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ServiceManager.Client.Components;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConnectToClusterViewModel : IInitable
    {
        private readonly IServerInfo _serverInfo;
        private readonly HttpClient _client;
        private readonly IEventAggregator _aggregator;
        private readonly IClusterConnectionTracker _tracker;

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

        public ConnectToClusterViewModel(IServerInfo serverInfo, HttpClient client, IEventAggregator aggregator, IClusterConnectionTracker tracker)
        {
            _serverInfo = serverInfo;
            _client = client;
            _aggregator = aggregator;
            _tracker = tracker;
        }

        public async Task ConnectToCluster()
        {
            using (Operation.Start())
            {
                try
                {
                    if (ValidateUrl(ClusterUrl) != null) return;

                    var result = await _tracker.ConnectToCluster(ClusterUrl);

                    if (result != string.Empty) return;

                    await _serverInfo.Restart();
                }
                catch (Exception e)
                {
                    _aggregator.PublishError(e);
                }
            }
        }

        public Task Init() 
            => PropertyChangedComponent.Init(_serverInfo);
    }
}