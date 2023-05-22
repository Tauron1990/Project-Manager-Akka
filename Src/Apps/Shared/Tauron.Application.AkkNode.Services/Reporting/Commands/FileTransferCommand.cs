using System;
using FluentResults;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.FileTransfer;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

[PublicAPI]
public abstract record FileTransferCommand<TSender, TThis> : ReporterCommandBase<TSender, TThis>
    where TThis : FileTransferCommand<TSender, TThis>
    where TSender : ISender
{
    [UsedImplicitly]
    public DataTransferManager? Manager { get; set; }

    public Result<DataTransferManager> GetTransferManager()
    {
        if(Manager is null)
            return Result.Fail("Transfer manager not Set");

        return Manager;
    }
}