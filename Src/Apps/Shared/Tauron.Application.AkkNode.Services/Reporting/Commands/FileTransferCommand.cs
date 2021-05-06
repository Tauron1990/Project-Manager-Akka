using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.FileTransfer;

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

    [PublicAPI]
    public static class FileTransferCommandExtension
    {
        public static Task<TransferMessages.TransferCompled> Send<TSender, TCommand>(this TSender sender, TCommand command, TimeSpan timeout,
            DataTransferManager manager, Action<string> messages, Func<Stream?> getdata)
            where TSender : ISender
            where TCommand : FileTransferCommand<TSender, TCommand>
            => Send(sender, command, timeout, manager, messages, () =>
            {
                var str = getdata();
                return str == null ? null : new StreamData(str);
            });

        public static async Task<TransferMessages.TransferCompled> Send<TSender, TCommand>(this TSender sender,
            TCommand command, TimeSpan timeout, DataTransferManager manager, Action<string> messages, Func<ITransferData?> getdata)
            where TSender : ISender
            where TCommand : FileTransferCommand<TSender, TCommand>
        {
            command.Manager = manager;
            var id = await SendingHelper.Send<FileTransactionId, TCommand>(sender, command, messages, timeout, false);

            var tranfer = await command.Manager.AskAwaitOperation(new AwaitRequest(timeout, id.Id));

            return await tranfer.TryStart(getdata);
        }
    }
}