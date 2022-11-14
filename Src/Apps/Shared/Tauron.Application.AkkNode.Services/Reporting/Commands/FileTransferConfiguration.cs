using System;
using System.Threading;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public sealed record FileTransferConfiguration<TSender, TCommand>(
    TSender Sender, TCommand Command, in Duration? Timeout, DataTransferManager DataTransferManager,
    Action<string>? Messages, Func<ITransferData?> CreateStream, CancellationToken CancellationToken = default);