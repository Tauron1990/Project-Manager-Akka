using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Server.AppCore.ClusterTracking.Data
{
    public interface INodeRepository
    {
        Task UpdateClusterNode(string url, Func<ClusterNodeInfo, ClusterNodeInfo> update);
        Task<ClusterNodeInfo> GetInfo(string url);
        Task Remove(string url);
        Task Add(ClusterNodeInfo info);

        Task<string[]> AllUrls();
    }

    public sealed class NodeRepository : INodeRepository
    {
        private readonly ConcurrentDictionary<string, ClusterNodeInfo> _infos = new();

        public Task UpdateClusterNode(string url, Func<ClusterNodeInfo, ClusterNodeInfo> update)
        {
            try
            {
                _infos[url] = update(_infos[url]);

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public Task<ClusterNodeInfo> GetInfo(string url)
        {
            try
            {
                return Task.FromResult(_infos[url]);
            }
            catch (Exception e)
            {
                return Task.FromException<ClusterNodeInfo>(e);
            }
        }

        public Task Remove(string url)
        {
            try
            {
                _infos.TryRemove(url, out _);

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public Task Add(ClusterNodeInfo info)
        {
            try
            {
                if (_infos.ContainsKey(info.Url)) return Task.CompletedTask;

                _infos[info.Url] = info;

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public Task<string[]> AllUrls()
            => Task.FromResult(_infos.Keys.ToArray());
    }
}