using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Server.Hubs
{ 
    public sealed class ClusterInfoHub : Hub
    {
        private readonly IClusterNodeManager _manager;

        public ClusterInfoHub(IClusterNodeManager manager) => _manager = manager;

        public Task SentPropertyChanged(string type, string name)
            => Clients.All.SendAsync(HubEvents.PropertyChanged, type, name);

        [HubMethodName(HubEvents.QueryNodes)]
        [UsedImplicitly]
        public async Task<NodeChange[]> QueryAllNodes()
        {
            var response = await _manager.QueryNodes();
            return response.Infos.Select(c => new NodeChange(c, false)).ToArray();
        }
    }
}