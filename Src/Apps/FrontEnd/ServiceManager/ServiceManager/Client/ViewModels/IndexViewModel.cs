using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Client.Components;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public class IndexViewModel : ObservableObject, IInitable
    {
        private readonly HubConnection _connection;
        private ImmutableDictionary<string, ClusterNodeInfo> _nodeInfos = ImmutableDictionary<string, ClusterNodeInfo>.Empty;
        private bool _init;

        public ImmutableDictionary<string, ClusterNodeInfo> NodeInfos
        {
            get => _nodeInfos;
            set => SetProperty(ref _nodeInfos, value);
        }

        public IndexViewModel(HubConnection connection)
        {
            _connection = connection;
            connection.On<NodeChange[]>(HubEvents.NodesChanged,
                info =>
                {
                    NodeInfos = info.Aggregate(NodeInfos, (current1, nodeChange) => nodeChange.Remove 
                                                              ? current1.Remove(nodeChange.Info.Url) 
                                                              : current1.SetItem(nodeChange.Info.Url, nodeChange.Info));
                });
        }

        public async Task Init()
        {
            if (_init)
                return;
            _init = true;

            var nodes = await _connection.InvokeAsync<NodeChange[]>(HubEvents.QueryNodes);
            NodeInfos = nodes.Select(nc => nc.Info).ToImmutableDictionary(cni => cni.Url);
        }
    }
}