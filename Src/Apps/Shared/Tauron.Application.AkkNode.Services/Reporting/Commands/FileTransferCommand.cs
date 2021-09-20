using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands
{
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
                if (_manager != null)
                    throw new InvalidOperationException("Datamanager Should set only once");
                _manager = value;
            }
        }

        public DataTransferManager GetTransferManager()
        {
            if (Manager == null)
                throw new InvalidOperationException("Transfer manager not Set");

            return Manager;
        }
    }

    public sealed record FileTransferConfiguration<TSender, TCommand>(
        TSender Sender, TCommand Command, TimeSpan Timeout, DataTransferManager DataTransferManager,
        Action<string>? Messages, Func<ITransferData?> CreateStream, CancellationToken CancellationToken = default);
    
    [PublicAPI]
    public static class FileTransferCommandExtension
    {
        public static FileTransferConfiguration<TSender, TCommand> NewFileTransfer<TSender, TCommand>(
            this TSender sender, TCommand command, TimeSpan timeout, DataTransferManager transferManager, Func<Stream?> data)
            where TSender : ISender
            where TCommand : FileTransferCommand<TSender, TCommand>
            => NewFileTransfer(sender, command, timeout, transferManager,
                () =>
                {
                    var str = data();

                    return str is null ? null : new StreamData(str);
                });

        public static FileTransferConfiguration<TSender, TCommand> NewFileTransfer<TSender, TCommand>(
            this TSender sender, TCommand command, TimeSpan timeout, DataTransferManager transferManager, Func<ITransferData?> data)
            where TSender : ISender
            where TCommand : FileTransferCommand<TSender, TCommand>
            => new(sender, command, timeout, transferManager, null, data);
        
        public static async Task<Either<TransferMessages.TransferCompled, Error>> Send<TSender, TCommand>(this FileTransferConfiguration<TSender, TCommand> buildCommand)
            where TSender : ISender
            where TCommand : FileTransferCommand<TSender, TCommand>
        {
            var (sender, command, timeout, manager, messages, getdata, cancelToken) = buildCommand;
            
            command.Manager = manager;
            var idEither = await SendingHelper.Send<FileTransactionId, TCommand>(sender, command, messages ?? (_ =>{}) , timeout, isEmpty: false, cancelToken);

            if (idEither.IsRight)
                return Either.Right((Error)idEither.Value);

            var id = (FileTransactionId)idEither.Value;
            
            var tranfer = await command.Manager.AskAwaitOperation(new AwaitRequest(timeout, id.Id));

            return Either.Left(await tranfer.TryStart(getdata));
        }
    }
}