using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ServiceManager.Client.Components;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConnectToClusterViewModel : IInitable
    {
        private readonly IServerInfo _serverInfo;
        private readonly HttpClient _client;
        private readonly IEventAggregator _aggregator;

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

        public ConnectToClusterViewModel(IServerInfo serverInfo, HttpClient client, IEventAggregator aggregator)
        {
            _serverInfo = serverInfo;
            _client = client;
            _aggregator = aggregator;
        }

        public async Task ConnectToCluster()
        {
            try
            {
                if(ValidateUrl(ClusterUrl) != null) return;

                using var response = await _client.PostAsJsonAsync(ClusterConnectionTrackerApi.ConnectToCluster, new StringApiContent(ClusterUrl));
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<StringApiContent>();

                if (result == null)
                {
                    _aggregator.PublishError($"Unbekanter Fehler beim verbinden mit Cluster: Statuscode: {response.StatusCode}");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(result.Content))
                {
                    _aggregator.PublishError($"Fehler bem verbinden mit Cluster: {result.Content}");
                    return;
                }

                await _serverInfo.Restart();
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public Task Init() 
            => PropertyChangedComponent.Init(_serverInfo);
    }
}