﻿using System;
using System.Threading.Tasks;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public sealed class AwaitResponse
{
    private readonly IncomingDataTransfer? _request;

    public AwaitResponse(IncomingDataTransfer? request) => _request = request;

    public async Task<TransferMessages.TransferCompled> TryStart(Func<ITransferData?> getdata)
    {
        if (_request is null)
            return new TransferFailed(FileOperationId.Empty, FailReason.Deny, TransferData.Empty);

        var data = getdata();

        if (data is null)
            return new TransferFailed(_request.OperationId, FailReason.Deny, _request.Data);

        return await _request.Accept(() => data);
    }
}

public sealed class AwaitRequest
{
    public AwaitRequest(Duration? timeout, FileOperationId id)
    {
        Timeout = timeout;
        Id = id;
    }

    public Duration? Timeout { get; }

    public FileOperationId Id { get; }
}