using Tauron.Application.AkkaNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed record TransferRepository(string RepoName, string OperationId) : FileTransferCommand<RepositoryApi, TransferRepository>, IRepositoryAction
    {
        protected override string Info => RepoName;
    }
}