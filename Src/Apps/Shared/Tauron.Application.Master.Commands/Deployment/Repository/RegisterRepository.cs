﻿using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository;

public sealed record RegisterRepository(RepositoryName RepoName, bool IgnoreDuplicate) : SimpleCommand<RepositoryApi, RegisterRepository>, IRepositoryAction
{
    protected override string Info => RepoName.Value;
}