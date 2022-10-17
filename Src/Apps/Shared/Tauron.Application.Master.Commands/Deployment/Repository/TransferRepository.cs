using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository;

public sealed record TransferRepository(RepositoryName RepoName) : FileTransferCommand<RepositoryApi, TransferRepository>, IRepositoryAction
{
    protected override string Info => RepoName.Value;
}