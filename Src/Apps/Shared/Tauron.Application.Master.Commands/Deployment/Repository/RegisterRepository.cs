﻿using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed record RegisterRepository(string RepoName, bool IgnoreDuplicate) : SimpleCommand<RepositoryApi, RegisterRepository>, IRepositoryAction
    {
        protected override string Info => RepoName;
    }
}