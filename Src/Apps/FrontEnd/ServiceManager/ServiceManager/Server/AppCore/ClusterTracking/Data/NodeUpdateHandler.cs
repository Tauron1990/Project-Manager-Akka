using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Shared.ClusterTracking;
using Stl.Async;
using Stl.Fusion;

namespace ServiceManager.Server.AppCore.ClusterTracking.Data
{
    public class NodeUpdateHandler : INodeUpdateHandler
    {
        private readonly INodeRepository _repository;
        private readonly IClusterNodeTracking _tracker;

        public NodeUpdateHandler(INodeRepository repository, IClusterNodeTracking tracker)
        {
            _repository = repository;
            _tracker = tracker;
        }

        public virtual async Task<Unit> AddNode(AddNodeCommad command, CancellationToken token = default)
        {
            await _repository.Add(new ClusterNodeInfo("Abrufen", command.ServiceType, command.Status, command.Url));
            using (Computed.Invalidate())
            {
                _tracker.GetInfo(command.Url).Ignore();
                _tracker.GetUrls().Ignore();
            }
            return Unit.Default;
        }

        public virtual async Task<Unit> RemoveNode(RemoveNodeCommand command, CancellationToken token = default)
        {
            await _repository.Remove(command.Url);
            using (Computed.Invalidate())
            {
                _tracker.GetInfo(command.Url).Ignore();
                _tracker.GetUrls().Ignore();
            }
            return Unit.Default;
        }

        public virtual async Task<Unit> UpdateName(UpdateNameCommand command, CancellationToken token = default)
        {
            await _repository.UpdateClusterNode(command.Url, info => info with { Name = command.Name, ServiceType = command.ServiceType});
            using (Computed.Invalidate()) 
                _tracker.GetInfo(command.Url).Ignore();

            return Unit.Default;
        }

        public virtual async Task<Unit> UpdateStatus(UpdateStatusCommand command, CancellationToken token = default)
        {
            await _repository.UpdateClusterNode(command.Url, c => c with { Status = command.Status });
            using (Computed.Invalidate())
                _tracker.GetInfo(command.Url).Ignore();
            
            return Unit.Default;
        }
    }
}