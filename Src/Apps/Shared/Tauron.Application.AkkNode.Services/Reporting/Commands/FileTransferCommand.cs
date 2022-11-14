using System;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.FileTransfer;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

[PublicAPI]
public abstract record FileTransferCommand<TSender, TThis> : ReporterCommandBase<TSender, TThis>
    where TThis : FileTransferCommand<TSender, TThis>
    where TSender : ISender
{
    private DataTransferManager? _manager;

    [UsedImplicitly]
    public DataTransferManager? Manager
    {
        get => _manager;
        set
        {
            if(_manager != null)
                throw new InvalidOperationException("Datamanager Should set only once");

            _manager = value;
        }
    }

    public DataTransferManager GetTransferManager()
    {
        if(Manager is null)
            throw new InvalidOperationException("Transfer manager not Set");

        return Manager;
    }
}