using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Operations;
using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

[PublicAPI]
public static class FileTransferCommandExtension
{
    public static FileTransferConfiguration<TSender, TCommand> NewFileTransfer<TSender, TCommand>(
        this TSender sender, TCommand command, in Duration? timeout, DataTransferManager transferManager, Func<Stream?> data)
        where TSender : ISender
        where TCommand : FileTransferCommand<TSender, TCommand>
        => NewFileTransfer(
            sender,
            command,
            timeout,
            transferManager,
            () =>
            {
                Stream? str = data();

                return str is null ? null : new StreamData(str);
            });

    public static FileTransferConfiguration<TSender, TCommand> NewFileTransfer<TSender, TCommand>(
        this TSender sender, TCommand command, in Duration? timeout, DataTransferManager transferManager, Func<ITransferData?> data)
        where TSender : ISender
        where TCommand : FileTransferCommand<TSender, TCommand>
        => new(sender, command, timeout, transferManager, Messages: null, data);

    public static async Task<Either<TransferMessages.TransferCompled, Error>> Send<TSender, TCommand>(this FileTransferConfiguration<TSender, TCommand> buildCommand)
        where TSender : ISender
        where TCommand : FileTransferCommand<TSender, TCommand>
    {
        (
            TSender sender,
            TCommand command,
            var timeout,
            DataTransferManager manager,
            var messages,
            var getdata,
            CancellationToken cancelToken) = buildCommand;

        command.Manager = manager;
        var idEither = await SendingHelper.Send<FileTransactionId, TCommand>(sender, command, messages ?? (_ => { }), timeout, isEmpty: false, cancelToken).ConfigureAwait(false);

        if(idEither.IsRight)
            return Either.Right((Error)idEither.Value);

        var id = (FileTransactionId)idEither.Value;

        AwaitResponse tranfer = await command.Manager.AskAwaitOperation(new AwaitRequest(timeout, FileOperationId.From(id.Id))).ConfigureAwait(false);

        return Either.Left(await tranfer.TryStart(getdata).ConfigureAwait(false));
    }
}