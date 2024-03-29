﻿using System;
using Stl.CommandR;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;

namespace ServiceManager.Shared.Apps
{
    public sealed record RunAppSetupCommand(int Step, string OperationId) : ICommand<RunAppSetupResponse>
    {
        public static RunAppSetupCommand Initial(string id)
            => new(0, id);
    }

    public sealed record RunAppSetupResponse(int NextStep, string? Error, bool IsCompled, string Message)
    {
        public RunAppSetupCommand CreateNextStep(string id)
        {
            if (IsCompled)
                throw new InvalidOperationException("No next Steps");

            return string.IsNullOrWhiteSpace(Error)
                ? new RunAppSetupCommand(NextStep, id)
                : throw new InvalidOperationException(Error);
        }
    }

    public sealed record QueryRepositoryResult(bool IsOk, string Error);
    
    public sealed record ApiCreateAppCommand(AppName Name, ProjectName ProjectName, RepositoryName RepositoryName) : ICommand<string>;

    public sealed record ApiDeleteAppCommand(AppName Name) : ICommand<string>;
}