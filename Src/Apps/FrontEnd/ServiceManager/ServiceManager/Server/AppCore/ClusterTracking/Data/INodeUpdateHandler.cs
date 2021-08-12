using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR.Configuration;

namespace ServiceManager.Server.AppCore.ClusterTracking.Data
{
    public interface INodeUpdateHandler
    {
        [CommandHandler]
        Task<Unit> AddNode(AddNodeCommad command, CancellationToken token = default);

        [CommandHandler]
        Task<Unit> RemoveNode(RemoveNodeCommand command, CancellationToken token = default);

        [CommandHandler]
        Task<Unit> UpdateName(UpdateNameCommand command, CancellationToken token = default);

        [CommandHandler]
        Task<Unit> UpdateStatus(UpdateStatusCommand command, CancellationToken token = default);
    }
}