using System;
using System.Threading.Tasks;
using ServiceManager.Server.AppCore.ClusterTracking.Data;
using ServiceManager.Shared.ClusterTracking;
using Stl.Fusion;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public class ClusterNodeTracking : IClusterNodeTracking
    {
        private readonly INodeRepository _repository;

        public ClusterNodeTracking(INodeRepository repository)
        {
            _repository = repository;
        }

        public virtual async Task<ClusterNodeInfo> GetInfo(string url)
        {
            if (Computed.IsInvalidating()) return null!;

            return await _repository.GetInfo(url);
        }

        public virtual async Task<string[]> GetUrls()
        {
            if (Computed.IsInvalidating()) return Array.Empty<string>();

            return await _repository.AllUrls();
        }
    }
}