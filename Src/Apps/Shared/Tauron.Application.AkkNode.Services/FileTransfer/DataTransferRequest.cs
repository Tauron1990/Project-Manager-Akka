﻿using System;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

[PublicAPI]
public abstract class DataTransferRequest : TransferMessages.TransferMessage
{
    protected DataTransferRequest(FileOperationId operationId, DataTransferManager target, TransferData data)
        : base(operationId)
    {
        Target = target;
        Data = data;
    }

    public abstract Func<ITransferData> Source { get; }

    public DataTransferManager Target { get; }

    public TransferData Data { get; }

    public object? IndividualMessage { get; set; }

    public bool SendCompletionBack { get; set; }

    public DataTransferRequest Inform(object? specific = null, bool sendBack = false)
    {
        SendCompletionBack = sendBack;
        IndividualMessage = specific;

        return this;
    }

    public static DataTransferRequest FromStream(Func<ITransferData> stream, DataTransferManager target, in TransferData? data = null)
        => new StreamTransferRequest(FileOperationId.New(), stream, target, data ?? TransferData.Empty);

    public static DataTransferRequest FromFile(string file, DataTransferManager target, in TransferData? data = null)
        => new DataStreamTranferRequest(FileOperationId.New(), file, target, data ?? TransferData.Empty);

    public static DataTransferRequest FromStream(Stream stream, DataTransferManager target, in TransferData? data = null)
        => new StreamTransferRequest(FileOperationId.New(), stream, target, data ?? TransferData.Empty);

    public static DataTransferRequest FromStream(Func<Stream> stream, DataTransferManager target, in TransferData? data = null)
        => new StreamTransferRequest(FileOperationId.New(), stream, target, data ?? TransferData.Empty);

    public static DataTransferRequest FromFile(in FileOperationId opsId, string file, DataTransferManager target, in TransferData? data = null)
        => new DataStreamTranferRequest(opsId, file, target, data ?? TransferData.Empty);

    public static DataTransferRequest FromStream(in FileOperationId opsId, Stream stream, DataTransferManager target, in TransferData? data = null)
        => new StreamTransferRequest(opsId, stream, target, data ?? TransferData.Empty);

    public static DataTransferRequest FromStream(in FileOperationId opsId, Func<Stream> stream, DataTransferManager target, in TransferData? data = null)
        => new StreamTransferRequest(opsId, stream, target, data ?? TransferData.Empty);

    public sealed class StreamTransferRequest : DataTransferRequest
    {
        public StreamTransferRequest(FileOperationId operationId, Func<Stream> source, DataTransferManager target, TransferData data)
            : base(operationId, target, data) =>
            Source = () => new StreamData(source());

        public StreamTransferRequest(FileOperationId operationId, Func<ITransferData> source, DataTransferManager target, TransferData data)
            : base(operationId, target, data) =>
            Source = source;

        public StreamTransferRequest(FileOperationId operationId, Stream source, DataTransferManager target, TransferData data)
            : this(operationId, () => source, target, data) { }

        public override Func<ITransferData> Source { get; }
    }

    public sealed class DataStreamTranferRequest : DataTransferRequest
    {
        public DataStreamTranferRequest(FileOperationId operationId, string file, DataTransferManager target, TransferData data)
            : base(operationId, target, data)
        {
            Source = () => new StreamData(File.OpenRead(file));
        }

        public override Func<ITransferData> Source { get; }
    }
}