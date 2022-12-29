using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Tauron.Servicemnager.Networking.IPC.Core;

internal class ReaderWriterHandler : IDisposable
{
    private const int ProtocolLen = 25;
    private readonly int _bufferLenS;
    private readonly Queue<byte[]> _bytesQueue = new();
    private readonly object _lockQ = new();

    private readonly SharedMemory _sm;
    private byte[]? _chunksCollected;

    //ulong MsgId_Received = 0;
    private ushort _currentChunk;

    private SWaitHadle _ewhReaderReadyToRead;
    private SWaitHadle _ewhReaderReadyToWrite;

    //EventWaitHandle ewh_Writer_ReadyToRead = null;
    private SWaitHadle _ewhWriterReadyToRead;
    private SWaitHadle _ewhWriterReadyToWrite;

    //ConcurrentQueue
    private bool _inSend;

    //ManualResetEvent mre_writer_thread = new ManualResetEvent(false);
    private AsyncManualResetEvent _mreWriterThread = new();
    private ulong _msgIdSending;

    private MemoryMappedViewAccessor _readerAccessor;
    private unsafe byte* _readerAccessorPtr = (byte*)0;
    private MemoryMappedFile _readerMmf;
    private byte[]? _toSend;

    /*Protocol
     * 1byte - MsgType. StandardMsg value is 1 eMsgType
     * Prot for MsgType 1 
     * 8bytes - msgId (ulong)
     * 4bytes - payload length (int)
     * 2bytes - currentChunk
     * 2bytes - totalChunks  //ChunksLeft (ushort) (if there is only 1 chunk, then chunks left will be 0. if there are 2 chunks: first will be 1 then will be 0)
     * 8bytes - responseMsgId
     * payload
     */


    /*Protocol V2         
    * Nbytes - protobuf int - MsgType. StandardMsg value is 1 eMsgType (normally 1 byte)        
    * Nbytes - protobuf ulong  - msgId (ulong)
    * Nbytes - protobuf int  - payload length
    * Nbytes - currentChunkNr
    * Nbytes - totalChunks  //ChunksLeft (ushort) (if there is only 1 chunk, then chunks left will be 0. if there are 2 chunks: first will be 1 then will be 0)
    * Nbytes - protobuf ulong - responseMsgId
    * payload
    * ....
    * next message
*/

    private int _totalBytesInQUeue;
    private MemoryMappedViewAccessor _writerAccessor;
    private unsafe byte* _writerAccessorPtr = (byte*)0;
    private MemoryMappedFile _writerMmf;


    #pragma warning disable CS8618
    internal ReaderWriterHandler(SharedMemory sm)
        #pragma warning restore CS8618
    {
        _sm = sm;
        //this.DataArrived = DataArrived;
        _bufferLenS = Convert.ToInt32(sm.BufferCapacity) - ProtocolLen;

        InitWriter();
        InitReader();

        //SendProcedure1();
    }

    ///// <summary>
    ///// MsgId of the sender and payload
    ///// </summary>
    //Action<eMsgType, ulong, byte[]> DataArrived = null;


    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _ewhWriterReadyToRead?.Close();
        _ewhWriterReadyToRead?.Dispose();
        _ewhWriterReadyToRead = null!;

        _ewhWriterReadyToWrite?.Close();
        _ewhWriterReadyToWrite?.Dispose();
        _ewhWriterReadyToWrite = null!;

        _ewhReaderReadyToRead?.Close();
        _ewhReaderReadyToRead?.Dispose();
        _ewhReaderReadyToRead = null!;

        _ewhReaderReadyToWrite?.Close();
        _ewhReaderReadyToWrite?.Dispose();
        _ewhReaderReadyToWrite = null!;

        _mreWriterThread?.Set();
        _mreWriterThread = null!;

        _writerAccessor?.SafeMemoryMappedViewHandle?.ReleasePointer();
        _writerAccessor?.Dispose();
        _writerAccessor = null!;

        _readerAccessor?.SafeMemoryMappedViewHandle.ReleasePointer();
        _readerAccessor?.Dispose();
        _readerAccessor = null!;

        _writerMmf?.Dispose();
        _writerMmf = null!;

        _readerMmf?.Dispose();
        _readerMmf = null!;
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }


    private unsafe void InitWriter()
    {
        string prefix = _sm.InstanceType == EInstanceType.Master ? "1" : "2";


        _ewhWriterReadyToRead = new SWaitHadle(
            false,
            EventResetMode.ManualReset,
            $"{_sm.UniqueHandlerName}{prefix}_SharmNet_ReadyToRead");

        _ewhWriterReadyToWrite = new SWaitHadle(
            true,
            EventResetMode.ManualReset,
            $"{_sm.UniqueHandlerName}{prefix}_SharmNet_ReadyToWrite");

        _ewhWriterReadyToWrite.Set();


        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _writerMmf = MemoryMappedFile.CreateOrOpen(
                $"{_sm.UniqueHandlerName}{prefix}_SharmNet_MMF",
                _sm.BufferCapacity,
                MemoryMappedFileAccess.ReadWrite,
                MemoryMappedFileOptions.DelayAllocatePages,
                HandleInheritability.Inheritable);
        }
        else
        {
            string name = Path.Combine(Path.GetTempPath(), $"{_sm.UniqueHandlerName}{prefix}_SharmNet_MMF.mem");
            _writerMmf = MemoryMappedFile.CreateFromFile(name, FileMode.OpenOrCreate, null, _sm.BufferCapacity, MemoryMappedFileAccess.ReadWrite);
        }

        _writerAccessor = _writerMmf.CreateViewAccessor(0, _sm.BufferCapacity);
        _writerAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _writerAccessorPtr);

    }

    /// <summary>
    ///     To get new Id this function must be used
    /// </summary>
    /// <returns></returns>
    internal ulong GetMessageId()
    {
        lock (_lockQ)
        {
            return ++_msgIdSending;
        }
    }

    /// <summary>
    ///     Returns false if buffer threshold is reached
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msgId"></param>
    /// <param name="msg"></param>
    /// <param name="responseMsgId"></param>
    /// <returns></returns>
    #pragma warning disable MA0051
    // ReSharper disable CognitiveComplexity
    internal bool SendMessage(EMsgType msgType, ulong msgId, byte[]? msg, ulong responseMsgId = 0)
    {
        if(Interlocked.Read(ref _sm.SharmIpc.Disposed) == 1)
            return false;

        if(_totalBytesInQUeue > _sm.MaxQueueSizeInBytes)
        {
            //Cleaning queue
            //lock (lock_q)
            //{
            //    totalBytesInQUeue = 0;
            //    q.Clear();
            //}
            //Generating exception

            //--STAT
            _sm.SharmIpc.Statistic.TotalBytesInQueueError();

            #pragma warning disable MA0076
            _sm.SharmIpc.LogException(
                $"tiesky.com.SharmIpc.ReaderWriterHandler.SendMessage: max queue treshold is reached {_sm.MaxQueueSizeInBytes}",
                new InvalidOperationException(
                    $"ReaderWriterHandler max queue treshold is reached {_sm.MaxQueueSizeInBytes}" +
                    $"; totalBytesInQUeue: {_totalBytesInQUeue}; q.Count: {_bytesQueue.Count}; " +
                    $"_ready2writeSignal_Last_Setup: {_sm.SharmIpc.Statistic.Ready2WriteSignalLastSetup}" +
                    $"_ready2ReadSignal_Last_Setup: {_sm.SharmIpc.Statistic.Ready2ReadSignalLastSetup}" +
                    _sm.SharmIpc.Statistic.Report()));
            #pragma warning restore MA0076

            //throw new Exception("tiesky.com.SharmIpc: ReaderWriterHandler max queue treshold is reached " + sm.maxQueueSizeInBytes);

            //Is handeld on upper level
            return false;
        }


        lock (_lockQ)
        {


            //Splitting message
            var i = 0;
            int left = msg?.Length ?? 0;

            ushort totalChunks = msg is null
                ? (ushort)1
                : msg.Length == 0
                    ? Convert.ToUInt16(1)
                    : Convert.ToUInt16(Math.Ceiling(msg.Length / (double)_bufferLenS));

            ushort currentChunk = 1;

            while (true)
            {
                byte[] pMsg;
                if(left > _bufferLenS)
                {

                    pMsg = new byte[_bufferLenS + ProtocolLen];

                    //Writing protocol header
                    Buffer.BlockCopy(new[] { (byte)msgType }, 0, pMsg, 0, 1); //MsgType (1 for standard message)
                    Buffer.BlockCopy(BitConverter.GetBytes(msgId), 0, pMsg, 1, 8); //msgId_Sending
                    Buffer.BlockCopy(BitConverter.GetBytes(_bufferLenS), 0, pMsg, 9, 4); //payload len
                    Buffer.BlockCopy(BitConverter.GetBytes(currentChunk), 0, pMsg, 13, 2); //current chunk
                    Buffer.BlockCopy(BitConverter.GetBytes(totalChunks), 0, pMsg, 15, 2); //total chunks
                    Buffer.BlockCopy(BitConverter.GetBytes(responseMsgId), 0, pMsg, 17, 8); //total chunks


                    //Writing payload
                    if(msg is { Length: > 0 })
                        Buffer.BlockCopy(msg, i, pMsg, ProtocolLen, _bufferLenS);

                    left -= _bufferLenS;
                    i += _bufferLenS;
                    _bytesQueue.Enqueue(pMsg);
                    _totalBytesInQUeue += pMsg.Length;
                }
                else
                {
                    pMsg = new byte[left + ProtocolLen];

                    //Writing protocol header
                    Buffer.BlockCopy(new[] { (byte)msgType }, 0, pMsg, 0, 1); //MsgType (1 for standard message)
                    Buffer.BlockCopy(BitConverter.GetBytes(msgId), 0, pMsg, 1, 8); //msgId_Sending
                    Buffer.BlockCopy(BitConverter.GetBytes(msg is { Length: 0 } ? int.MaxValue : left), 0, pMsg, 9, 4); //payload len
                    Buffer.BlockCopy(BitConverter.GetBytes(currentChunk), 0, pMsg, 13, 2); //current chunk
                    Buffer.BlockCopy(BitConverter.GetBytes(totalChunks), 0, pMsg, 15, 2); //total chunks
                    Buffer.BlockCopy(BitConverter.GetBytes(responseMsgId), 0, pMsg, 17, 8); //total chunks

                    //Writing payload
                    if(msg is { Length: > 0 })
                        Buffer.BlockCopy(msg, i, pMsg, ProtocolLen, left);

                    _bytesQueue.Enqueue(pMsg);
                    _totalBytesInQUeue += pMsg.Length;

                    break;
                }

                currentChunk++;
            }


            //mre_writer_thread.Set();

            _sm.SharmIpc.Statistic.TotalBytesInQueue = _totalBytesInQUeue;

        } //eo lock


        WriterV01();

        //StartSendProcedure();
        //StartSendProcedure_v2();


        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msgId"></param>
    /// <param name="msg"></param>
    /// <param name="responseMsgId"></param>
    /// <returns></returns>
    internal bool SendMessageV2(EMsgType msgType, ulong msgId, byte[]? msg, ulong responseMsgId = 0)
    {
        if(Interlocked.Read(ref _sm.SharmIpc.Disposed) == 1)
            return false;

        try
        {
            if(_totalBytesInQUeue > _sm.MaxQueueSizeInBytes)
            {

                //--STAT
                _sm.SharmIpc.Statistic.TotalBytesInQueueError();

                #pragma warning disable MA0075
                _sm.SharmIpc.LogException(
                    "tiesky.com.SharmIpc.ReaderWriterHandler.SendMessageV2: max queue treshold is reached" + _sm.MaxQueueSizeInBytes,
                    new Exception(
                        $"ReaderWriterHandler max queue treshold is reached {_sm.MaxQueueSizeInBytes}" +
                        $"; totalBytesInQUeue: {_totalBytesInQUeue}; q.Count: {_bytesQueue.Count}; " +
                        $"_ready2writeSignal_Last_Setup: {_sm.SharmIpc.Statistic.Ready2WriteSignalLastSetup}" +
                        $"_ready2ReadSignal_Last_Setup: {_sm.SharmIpc.Statistic.Ready2ReadSignalLastSetup}" +
                        _sm.SharmIpc.Statistic.Report()));
                #pragma warning restore MA0075

                //throw new Exception("tiesky.com.SharmIpc: ReaderWriterHandler max queue treshold is reached " + sm.maxQueueSizeInBytes);

                //Is handeld on upper level
                return false;
            }


            lock (_lockQ)
            {

                //Splitting message
                var i = 0;
                int left = msg?.Length ?? 0;

                ushort totalChunks = msg is null
                    ? (ushort)1
                    : msg.Length == 0
                        ? Convert.ToUInt16(1)
                        : Convert.ToUInt16(Math.Ceiling(msg.Length / (double)_bufferLenS));

                ushort currentChunk = 1;

                while (true)
                {

                    int currentChunkLen = left > _bufferLenS ? _bufferLenS : left;

                    //Writing protocol header
                    byte[] tmp = ((ulong)msgType).ToProtoBytes();
                    _totalBytesInQUeue += tmp.Length;
                    _bytesQueue.Enqueue(tmp); //MsgType (1 for standard message)                       
                    tmp = msgId.ToProtoBytes();
                    _totalBytesInQUeue += tmp.Length;
                    _bytesQueue.Enqueue(tmp); //msgId_Sending                      
                    tmp = ((ulong)currentChunkLen).ToProtoBytes();
                    _totalBytesInQUeue += tmp.Length;
                    _bytesQueue.Enqueue(tmp); //payload len  
                    tmp = ((ulong)currentChunk).ToProtoBytes();
                    _totalBytesInQUeue += tmp.Length;
                    _bytesQueue.Enqueue(tmp); //current chunk
                    tmp = ((ulong)totalChunks).ToProtoBytes();
                    _totalBytesInQUeue += tmp.Length;
                    _bytesQueue.Enqueue(tmp); //total chunks 
                    tmp = responseMsgId.ToProtoBytes();
                    _totalBytesInQUeue += tmp.Length;
                    _bytesQueue.Enqueue(tmp); //Response message id

                    //Writing payload
                    if(msg is { Length: > 0 })
                    {
                        tmp = new byte[currentChunkLen];
                        Buffer.BlockCopy(msg, i, tmp, 0, currentChunkLen);
                        _totalBytesInQUeue += tmp.Length;
                        _bytesQueue.Enqueue(tmp);
                    }


                    left -= currentChunkLen;
                    i += currentChunkLen;

                    if(left == 0)
                        break;

                    currentChunk++;
                }

                _sm.SharmIpc.Statistic.TotalBytesInQueue = _totalBytesInQUeue;

            } //eo lock

            WriterV02();

        }
        catch (Exception ex)
        {
            _sm.SharmIpc.LogException("SharmIps.ReaderWriterHandler.SendMessageV2", ex);

            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    private void WriterV01()
    {
        lock (_lockQ)
        {
            if(_inSend || !_bytesQueue.Any())
                return;

            _inSend = true;
            _toSend = _bytesQueue.Dequeue();
            _totalBytesInQUeue -= _toSend.Length;
            _sm.SharmIpc.Statistic.TotalBytesInQueue = _totalBytesInQUeue;

        }

        while (true)
        {

            //--STAT
            _sm.SharmIpc.Statistic.StartToWait_ReadyToWrite_Signal();

            if(_ewhWriterReadyToWrite.WaitOne()) //We don't need here async awaiter
            {
                //--STAT
                _sm.SharmIpc.Statistic.StopToWait_ReadyToWrite_Signal();

                if(Interlocked.Read(ref _sm.SharmIpc.Disposed) == 1)
                    return;

                _ewhWriterReadyToWrite.Reset();

                WriteBytes(0, _toSend);

                //Setting signal ready to read
                _ewhWriterReadyToRead.Set();

                lock (_lockQ)
                {
                    if(_bytesQueue.Count == 0)
                    {
                        _toSend = null;
                        _inSend = false;

                        return;
                    }

                    _toSend = _bytesQueue.Dequeue();
                    _totalBytesInQUeue -= _toSend.Length;
                    _sm.SharmIpc.Statistic.TotalBytesInQueue = _totalBytesInQUeue;
                }
            }
        } //eo while


    } //eof


    private void WriterV02()
    {
        var offset = 0;
        lock (_lockQ)
        {
            if(_inSend || !_bytesQueue.Any())
                return;

            _inSend = true;
        }

        while (true)
        {
            //--STAT
            _sm.SharmIpc.Statistic.StartToWait_ReadyToWrite_Signal();

            if(!_ewhWriterReadyToWrite.WaitOne()) //We don't need here async awaiter
                continue;

            //--STAT
            _sm.SharmIpc.Statistic.StopToWait_ReadyToWrite_Signal();

            if(Interlocked.Read(ref _sm.SharmIpc.Disposed) == 1)
                return;

            _ewhWriterReadyToWrite.Reset();

            lock (_lockQ)
            {
                using (var ms = new MemoryStream())
                {
                    while (true)
                    {
                        _toSend = _bytesQueue.Dequeue();
                        _totalBytesInQUeue -= _toSend.Length;

                        ms.Write(_toSend, 0, _toSend.Length);
                        offset += _toSend.Length;

                        if(offset >= _bufferLenS || !_bytesQueue.Any())
                            break;
                    }

                    //Sending complete size
                    //proto = ((ulong)offset).ToProtoBytes();
                    //WriteBytes(0, proto);
                    //WriteBytes(proto.Length, ms.ToArray());
                    byte[] proto = ((ulong)offset).ToProtoBytes().Concat(ms.ToArray());
                    WriteBytes(0, proto);

                    ms.Close();
                }

                _sm.SharmIpc.Statistic.TotalBytesInQueue = _totalBytesInQUeue;

                offset = 0;
                //Setting signal ready to read
                _ewhWriterReadyToRead.Set();

                if(_bytesQueue.Any())
                    continue;

                _toSend = null;
                _inSend = false;

                return;
            } //eo lock


            ////Setting signal ready to read
            //ewh_Writer_ReadyToRead.Set();
            //offset = 0;

            //lock (lock_q)
            //{
            //    if (q.Count() == 0)
            //    {
            //        toSend = null;
            //        inSend = false;
            //        return;
            //    }                       
            //}
        } //eo while


    } //eof


    private unsafe void WriteBytes(int offset, byte[] data)
    {
        ////--STAT
        //this.sm.SharmIPC.Statistic.Writing(data.Length);

        Marshal.Copy(data, 0, nint.Add(new nint(_writerAccessorPtr), offset), data.Length);

        //https://msdn.microsoft.com/en-us/library/system.io.memorymappedfiles.memorymappedviewaccessor.safememorymappedviewhandle(v=vs.100).aspx
    }

    private unsafe byte[] ReadBytes(int offset, int num)
    {
        ////--STAT
        //this.sm.SharmIPC.Statistic.Reading(num);

        var arr = new byte[num];
        Marshal.Copy(nint.Add(new nint(_readerAccessorPtr), offset), arr, 0, num);

        return arr;
    }

    /// <summary>
    /// </summary>
    private unsafe void InitReader()
    {
        string prefix = _sm.InstanceType == EInstanceType.Slave ? "1" : "2";

        _ewhReaderReadyToRead = new SWaitHadle(
            false,
            EventResetMode.ManualReset,
            $"{_sm.UniqueHandlerName}{prefix}_SharmNet_ReadyToRead");

        _ewhReaderReadyToWrite = new SWaitHadle(
            true,
            EventResetMode.ManualReset,
            $"{_sm.UniqueHandlerName}{prefix}_SharmNet_ReadyToWrite");

        _ewhReaderReadyToWrite.Set();


        //if (sm.instanceType == tiesky.com.SharmIpc.eInstanceType.Slave)
        //{
        //    Console.WriteLine("My reader handlers:");
        //    Console.WriteLine(sm.uniqueHandlerName + prefix + "_SharmNet_ReadyToRead");
        //    Console.WriteLine(sm.uniqueHandlerName + prefix + "_SharmNet_ReadyToWrite");
        //    Console.WriteLine("-------");
        //}

        //Reader_mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateOrOpen(sm.uniqueHandlerName + prefix + "_SharmNet_MMF", sm.bufferCapacity, MemoryMappedFileAccess.ReadWrite);
        //Reader_accessor = Reader_mmf.CreateViewAccessor(0, sm.bufferCapacity);


        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _readerMmf = MemoryMappedFile.CreateOrOpen(
                $"{_sm.UniqueHandlerName}{prefix}_SharmNet_MMF",
                _sm.BufferCapacity,
                MemoryMappedFileAccess.ReadWrite,
                MemoryMappedFileOptions.DelayAllocatePages,
                HandleInheritability.Inheritable);
        }
        else
        {
            string fileName = Path.Combine(Path.GetTempPath(), $"{_sm.UniqueHandlerName}{prefix}_SharmNet_MMF.mem");

            _readerMmf = MemoryMappedFile.CreateFromFile(
                fileName,
                FileMode.OpenOrCreate,
                null,
                _sm.BufferCapacity,
                MemoryMappedFileAccess.ReadWrite);
        }

        _readerAccessor = _readerMmf.CreateViewAccessor(0, _sm.BufferCapacity);
        //AcquirePointer();
        _readerAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _readerAccessorPtr);


        #pragma warning disable EPC13
        Task.Run(
            #pragma warning restore EPC13
            () =>
            {
                switch (_sm.ProtocolVersion)
                {
                    case SharmIpc.EProtocolVersion.V1:
                        #pragma warning disable CS4014
                        #pragma warning disable EPC13
                        ReaderV01();

                        break;
                    case SharmIpc.EProtocolVersion.V2:
                        ReaderV02();
                        #pragma warning restore EPC13
                        #pragma warning restore CS4014

                        break;

                }

            });

        //ReaderV01wrapper();

        //ReaderV01();
    }

    private async Task ReaderV02()
    {
        ushort iCurChunk = 0;
        ushort iTotChunk = 0;
        ulong iMsgId = 0;
        var iPayLoadLen = 0;
        ulong iResponseMsgId = 0;

        var msgType = EMsgType.RpcRequest;

        var jPos = 0;
        //byte[] jReadBytes = null;
        var gout = false;

        var sizer8 = new byte[8];
        var sizer4 = new byte[4];
        var sizer2 = new byte[2];
        var size = 0;

        void ClearSizer8()
        {
            sizer8[0] = 0;
            sizer8[1] = 0;
            sizer8[2] = 0;
            sizer8[3] = 0;
            sizer8[4] = 0;
            sizer8[5] = 0;
            sizer8[6] = 0;
            sizer8[7] = 0;

            size = 0;
        }

        void ClearSizer4()
        {
            sizer4[0] = 0;
            sizer4[1] = 0;
            sizer4[2] = 0;
            sizer4[3] = 0;

            size = 0;
        }

        void ClearSizer2()
        {
            sizer2[0] = 0;
            sizer2[1] = 0;

            size = 0;
        }

        try
        {
            while (true)
            {

                await WaitHandleAsyncFactory.FromWaitHandle(_ewhReaderReadyToRead).ConfigureAwait(false);

                //--STAT
                _sm.SharmIpc.Statistic.Stop_WaitForRead_Signal();

                if(Interlocked.Read(ref _sm.SharmIpc.Disposed) == 1)
                    return;

                //--STAT
                _sm.SharmIpc.Statistic.Start_ReadProcedure_Signal();

                //Setting STOP for ewh_Reader_ReadyToRead.WaitOne()
                _ewhReaderReadyToRead.Reset();

                //Reading multiple packages
                var iHdr = 0;
                var protPos = EProtocolPosition.Init;

                /*Protocol V2     
                    * 
                    * Nbytes - totalSentLenght
                    * 
                    * START MESSAGE CYCLE
                    * 
                    * Nbytes - protobuf int - MsgType. StandardMsg value is 1 eMsgType (normally 1 byte)        
                    * Nbytes - protobuf ulong  - msgId (ulong)
                    * Nbytes - protobuf int  - payload length
                    * Nbytes - currentChunkNr
                    * Nbytes - totalChunks  //ChunksLeft (ushort) (if there is only 1 chunk, then chunks left will be 0. if there are 2 chunks: first will be 1 then will be 0)
                    * Nbytes - protobuf ulong - responseMsgId
                    * payload
                    * ....
                    * next message
                    * ....
                    * * END MESSAGE CYCLE
                 */

                while (true)
                {
                    if(gout)
                    {
                        gout = false;

                        break;
                    }

                    byte[]? payload;
                    byte[] hdr = Array.Empty<byte>();
                    switch (protPos)
                    {
                        case EProtocolPosition.GoOut:
                            gout = true;

                            break;
                        case EProtocolPosition.Init:
                            payload = ReadBytes(0, 100); //Reading totalFileSize and a bit of the rest of the file
                            sizer4[size] = payload[size];
                            if((sizer4[size] & 0x80) > 0)
                            {
                                size++;
                            }
                            else
                            {
                                size++; //we will use it as length
                                var totalFileSize = Convert.ToInt32(sizer4.FromProtoBytes());

                                hdr = new byte[totalFileSize];

                                if(size + totalFileSize > payload.Length)
                                {
                                    int usedSpace = payload.Length - size;
                                    Buffer.BlockCopy(payload, size, hdr, 0, usedSpace);
                                    payload = ReadBytes(payload.Length, totalFileSize - usedSpace);
                                    Buffer.BlockCopy(payload, 0, hdr, usedSpace, payload.Length);
                                }
                                else
                                {
                                    Buffer.BlockCopy(payload, size, hdr, 0, totalFileSize);
                                }

                                ClearSizer4();
                                protPos = EProtocolPosition.MsgType;
                            }

                            break;
                        case EProtocolPosition.MsgType:
                            msgType = (EMsgType)hdr[iHdr];
                            iHdr++;
                            protPos = EProtocolPosition.MsgId;

                            break;
                        case EProtocolPosition.MsgId:
                            sizer8[size] = hdr[iHdr];
                            if((sizer8[size] & 0x80) > 0)
                            {
                                size++;
                            }
                            else
                            {
                                iMsgId = sizer8.FromProtoBytes();
                                ClearSizer8();
                                protPos = EProtocolPosition.PayloadLen;
                            }

                            iHdr++;

                            break;
                        case EProtocolPosition.PayloadLen:
                            sizer4[size] = hdr[iHdr];
                            if((sizer4[size] & 0x80) > 0)
                            {
                                size++;
                            }
                            else
                            {
                                iPayLoadLen = Convert.ToInt32(sizer4.FromProtoBytes());
                                ClearSizer4();
                                protPos = EProtocolPosition.CurrentChunk;
                            }

                            iHdr++;

                            break;
                        case EProtocolPosition.CurrentChunk:
                            sizer2[size] = hdr[iHdr];
                            if((sizer2[size] & 0x80) > 0)
                            {
                                size++;
                            }
                            else
                            {
                                iCurChunk = Convert.ToUInt16(sizer2.FromProtoBytes());
                                ClearSizer2();
                                protPos = EProtocolPosition.TotalChunks;
                            }

                            iHdr++;

                            break;
                        case EProtocolPosition.TotalChunks:
                            sizer2[size] = hdr[iHdr];
                            if((sizer2[size] & 0x80) > 0)
                            {
                                size++;
                            }
                            else
                            {
                                iTotChunk = Convert.ToUInt16(sizer2.FromProtoBytes());
                                ClearSizer2();
                                protPos = EProtocolPosition.ResponseMsgId;
                            }

                            iHdr++;

                            break;
                        case EProtocolPosition.ResponseMsgId:
                            sizer8[size] = hdr[iHdr];
                            if((sizer8[size] & 0x80) > 0)
                            {
                                size++;
                            }
                            else
                            {
                                iResponseMsgId = sizer8.FromProtoBytes();
                                ClearSizer8();
                                protPos = EProtocolPosition.Payload;
                            }

                            iHdr++;

                            break;
                        case EProtocolPosition.Payload:
                            //Reading payload    
                            if(iPayLoadLen == int.MaxValue)
                            {
                                payload = Array.Empty<byte>();
                            }
                            else if(iPayLoadLen == 0)
                            {
                                payload = null;
                            }
                            else
                            {
                                payload = new byte[iPayLoadLen];
                                Buffer.BlockCopy(hdr, iHdr, payload, 0, iPayLoadLen);
                                iHdr += iPayLoadLen;
                            }

                            protPos = iHdr >= hdr.Length ? EProtocolPosition.GoOut : EProtocolPosition.MsgType;
                            //Processing payload
                            switch (msgType)
                            {
                                case EMsgType.ErrorInRpc:
                                    _sm.SharmIpc.InternalDataArrived(msgType, iResponseMsgId, null);

                                    break;

                                case EMsgType.RpcResponse:
                                case EMsgType.RpcRequest:
                                case EMsgType.Request:

                                    jPos = 7;

                                    if(iCurChunk == 1)
                                    {
                                        _chunksCollected = null;
                                    }
                                    else if(iCurChunk != _currentChunk + 1)
                                    {
                                        //Wrong income, sending special signal back, waiting for new MsgId   
                                        switch (msgType)
                                        {
                                            case EMsgType.RpcRequest:
                                                SendMessage(EMsgType.ErrorInRpc, GetMessageId(), null, iMsgId);

                                                break;
                                            case EMsgType.RpcResponse:
                                                _sm.SharmIpc.InternalDataArrived(EMsgType.ErrorInRpc, iResponseMsgId, null);

                                                break;
                                        }

                                        //!!! Here complete get out (sending instance has restarted and started new msgId sending cycle)
                                        protPos = EProtocolPosition.GoOut;

                                        break;
                                    }

                                    byte[] ret;
                                    if(iTotChunk == iCurChunk)
                                    {
                                        jPos = 13;
                                        if(_chunksCollected is null)
                                        {
                                            jPos = 27;
                                            _sm.SharmIpc.InternalDataArrived(
                                                msgType,
                                                msgType == EMsgType.RpcResponse ? iResponseMsgId : iMsgId,
                                                payload);

                                            jPos = 15;
                                        }
                                        else
                                        {
                                            jPos = 16;
                                            ret = new byte[iPayLoadLen + _chunksCollected.Length];
                                            Buffer.BlockCopy(_chunksCollected, 0, ret, 0, _chunksCollected.Length);
                                            Buffer.BlockCopy(payload ?? Array.Empty<byte>(), 0, ret, _chunksCollected.Length, iPayLoadLen);
                                            _sm.SharmIpc.InternalDataArrived(msgType, msgType == EMsgType.RpcResponse ? iResponseMsgId : iMsgId, ret);
                                            jPos = 17;
                                        }

                                        _chunksCollected = null;
                                        _currentChunk = 0;
                                    }
                                    else
                                    {
                                        jPos = 18;
                                        if(_chunksCollected is null)
                                        {
                                            jPos = 19;
                                            _chunksCollected = payload;
                                            jPos = 20;
                                        }
                                        else
                                        {
                                            jPos = 21;
                                            ret = new byte[_chunksCollected.Length + iPayLoadLen];
                                            Buffer.BlockCopy(_chunksCollected, 0, ret, 0, _chunksCollected.Length);
                                            Buffer.BlockCopy(payload ?? Array.Empty<byte>(), 0, ret, _chunksCollected.Length, iPayLoadLen);
                                            _chunksCollected = ret;
                                            jPos = 22;
                                        }

                                        jPos = 23;
                                        _currentChunk = iCurChunk;
                                    }

                                    break;

                                default:
                                    //Unknown protocol type
                                    jPos = 24;
                                    _chunksCollected = null;
                                    _currentChunk = 0;

                                    //Wrong income, doing nothing
                                    #pragma warning disable EX006
                                    throw new InvalidOperationException("tiesky.com.SharmIpc: Reading protocol contains errors");
                                #pragma warning restore EX006
                                //break;
                            } //eo switch

                            break;
                    }


                } //end of package


                jPos = 25;
                //Setting signal 
                _ewhReaderReadyToWrite.Set();

                //--STAT
                _sm.SharmIpc.Statistic.Stop_ReadProcedure_Signal();

                //--STAT
                _sm.SharmIpc.Statistic.Start_WaitForRead_Signal();

                jPos = 26;
            }
        }
        catch (Exception ex)
        {
            //constrained execution region (CER)
            //https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.safehandle.dangerousaddref(v=vs.110).aspx

            _sm.SharmIpc.LogException("SharmIps.ReaderWriterHandler.ReaderV02 LE, jPos=" + jPos + "; ", ex);
        }


    } //eo Reader V1


    private async Task ReaderV01()
    {

        var jPos = 0;
        var jProtocolLen = 0;
        var jPayloadLen = 0;
        byte[]? jReadBytes = null;

        try
        {
            while (true)
            {
                jPos = 0;

                //Exchange these 2 lines to make Reader event wait in async mode
                //ewh_Reader_ReadyToRead.WaitOne();
                await WaitHandleAsyncFactory.FromWaitHandle(_ewhReaderReadyToRead).ConfigureAwait(false); //.ConfigureAwait(false);


                //--STAT
                _sm.SharmIpc.Statistic.Stop_WaitForRead_Signal();

                if(Interlocked.Read(ref _sm.SharmIpc.Disposed) == 1)
                    return;
                //jPos = 1;
                //if (ewh_Reader_ReadyToRead == null) //Special Dispose case
                //    return;
                //jPos = 2;

                //--STAT
                _sm.SharmIpc.Statistic.Start_ReadProcedure_Signal();

                //Setting STOP for ewh_Reader_ReadyToRead.WaitOne()
                _ewhReaderReadyToRead.Reset();


                //Reading data from MMF
                jPos = 3;
                //Reading header
                byte[] hdr = ReadBytes(0, ProtocolLen);
                jPos = 4;
                var msgType = (EMsgType)hdr[0];

                //Parsing header
                ulong iResponseMsgId;
                switch (msgType)
                {
                    case EMsgType.ErrorInRpc:
                        jPos = 5;
                        BitConverter.ToInt32(hdr, 9); //+4
                        iResponseMsgId = BitConverter.ToUInt64(hdr, 17); //+8

                        _sm.SharmIpc.InternalDataArrived(msgType, iResponseMsgId, null);
                        jPos = 6;

                        break;

                    case EMsgType.RpcResponse:
                    case EMsgType.RpcRequest:
                    case EMsgType.Request:

                        jPos = 7;
                        var zeroByte = false;
                        var iMsgId = BitConverter.ToUInt64(hdr, 1);
                        var iPayLoadLen = BitConverter.ToInt32(hdr, 9);
                        if(iPayLoadLen == int.MaxValue)
                        {
                            zeroByte = true;
                            iPayLoadLen = 0;
                        }

                        var iCurChunk = BitConverter.ToUInt16(hdr, 13);
                        var iTotChunk = BitConverter.ToUInt16(hdr, 15);
                        iResponseMsgId = BitConverter.ToUInt64(hdr, 17); //+8
                        jPos = 8;
                        if(iCurChunk == 1)
                        {
                            _chunksCollected = null;
                        }
                        else if(iCurChunk != _currentChunk + 1)
                        {
                            //Wrong income, sending special signal back, waiting for new MsgId   
                            switch (msgType)
                            {
                                case EMsgType.RpcRequest:
                                    jPos = 9;
                                    SendMessage(EMsgType.ErrorInRpc, GetMessageId(), null, iMsgId);
                                    jPos = 10;

                                    break;
                                case EMsgType.RpcResponse:
                                    jPos = 11;
                                    _sm.SharmIpc.InternalDataArrived(EMsgType.ErrorInRpc, iResponseMsgId, null);
                                    jPos = 12;

                                    break;
                            }

                            break;
                        }

                        if(iTotChunk == iCurChunk)
                        {
                            jPos = 13;
                            if(_chunksCollected is null)
                            {
                                jPos = 14;
                                //Was
                                //this.sm.SharmIPC.InternalDataArrived(msgType, (msgType == eMsgType.RpcResponse) ? iResponseMsgId : iMsgId, iPayLoadLen == 0 ? ((zeroByte) ? new byte[0] : null) : ReadBytes(Reader_accessor_ptr, protocolLen, iPayLoadLen));
                                jProtocolLen = ProtocolLen;
                                jPayloadLen = iPayLoadLen;
                                jReadBytes = ReadBytes(ProtocolLen, iPayLoadLen);
                                jPos = 27;
                                _sm.SharmIpc.InternalDataArrived(msgType, msgType == EMsgType.RpcResponse ? iResponseMsgId : iMsgId, iPayLoadLen == 0 ? zeroByte ? Array.Empty<byte>() : null : jReadBytes);
                                ///////////// test
                                jPos = 15;
                            }
                            else
                            {
                                jPos = 16;
                                var ret = new byte[iPayLoadLen + _chunksCollected.Length];
                                Buffer.BlockCopy(_chunksCollected, 0, ret, 0, _chunksCollected.Length);
                                Buffer.BlockCopy(ReadBytes(ProtocolLen, iPayLoadLen), 0, ret, _chunksCollected.Length, iPayLoadLen);
                                _sm.SharmIpc.InternalDataArrived(msgType, msgType == EMsgType.RpcResponse ? iResponseMsgId : iMsgId, ret);
                                jPos = 17;
                            }

                            _chunksCollected = null;
                            _currentChunk = 0;
                        }
                        else
                        {
                            jPos = 18;
                            if(_chunksCollected is null)
                            {
                                jPos = 19;
                                _chunksCollected = ReadBytes(ProtocolLen, iPayLoadLen);
                                jPos = 20;
                            }
                            else
                            {
                                jPos = 21;
                                var tmp = new byte[_chunksCollected.Length + iPayLoadLen];
                                Buffer.BlockCopy(_chunksCollected, 0, tmp, 0, _chunksCollected.Length);
                                Buffer.BlockCopy(ReadBytes(ProtocolLen, iPayLoadLen), 0, tmp, _chunksCollected.Length, iPayLoadLen);
                                _chunksCollected = tmp;
                                jPos = 22;
                            }

                            jPos = 23;
                            _currentChunk = iCurChunk;
                        }

                        break;
                    default:
                        //Unknown protocol type
                        jPos = 24;
                        _chunksCollected = null;
                        _currentChunk = 0;

                        //Wrong income, doing nothing
                        #pragma warning disable EX006
                        throw new InvalidOperationException("tiesky.com.SharmIpc: Reading protocol contains errors");
                    #pragma warning restore EX006
                    //break;
                } //eo switch

                jPos = 25;
                //Setting signal 
                _ewhReaderReadyToWrite.Set();

                //--STAT
                _sm.SharmIpc.Statistic.Stop_ReadProcedure_Signal();

                //--STAT
                _sm.SharmIpc.Statistic.Start_WaitForRead_Signal();

                jPos = 26;
            }
        }
        catch (Exception ex)
        {
            //latest jPos = 27
            /*
             *  int jProtocolLen = 0;
        int jPayloadLen = 0;
        byte[] jReadBytes = null;
             */
            /*					
            System.ObjectDisposedException: Das SafeHandle wurde geschlossen. bei System.Runtime.InteropServices.SafeHandle.DangerousAddRef(Boolean& success) 
            bei System.StubHelpers.StubHelpers.SafeHandleAddRef(SafeHandle pHandle, Boolean& success) 
            bei Microsoft.Win32.Win32Native.SetEvent(SafeWaitHandle handle) bei System.Threading.EventWaitHandle.Set() bei tiesky.com.SharmIpcInternals.ReaderWriterHandler.b__28_0()
            */

            /*
             System.ObjectDisposedException: Das SafeHandle wurde geschlossen. bei System.Runtime.InteropServices.SafeHandle.DangerousAddRef(Boolean& success) 
             bei Microsoft.Win32.Win32Native.SetEvent(SafeWaitHandle handle)  
             bei System.Threading.EventWaitHandle.Set() bei tiesky.com.SharmIpcInternals.ReaderWriterHandler.b__28_0()
             */

            //constrained execution region (CER)
            //https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.safehandle.dangerousaddref(v=vs.110).aspx

            _sm.SharmIpc.LogException("SharmIps.ReaderWriterHandler.InitReader LE, jPos=" + jPos + "; jProtLen=" + jProtocolLen + "; jPaylLen=" + jPayloadLen + "; jReadBytesLen=" + (jReadBytes?.Length ?? 0), ex);
        }


    } //eo Reader V1


    private enum EProtocolPosition
    {
        Init,
        MsgType,
        MsgId,
        PayloadLen,
        CurrentChunk,
        TotalChunks,
        ResponseMsgId,
        Payload,
        GoOut,
    }


    //async Task ReaderV01_2(byte[] uhdr)
    //{
    //    byte[] hdr = null;
    //    byte[] ret = null;
    //    ushort iCurChunk = 0;
    //    ushort iTotChunk = 0;
    //    ulong iMsgId = 0;
    //    int iPayLoadLen = 0;
    //    ulong iResponseMsgId = 0;

    //    eMsgType msgType = eMsgType.RpcRequest;
    //    int jPos = 0;
    //    int jProtocolLen = 0;
    //    int jPayloadLen = 0;
    //    byte[] jReadBytes = null;
    //    int msgOffset = 3;
    //    ushort qMsg = 0;

    //    try
    //    {
    //        while (true)
    //        {
    //            msgOffset = 3;

    //            if(uhdr == null)
    //            {
    //                //if (sm.instanceType == eInstanceType.Slave)
    //                //    Console.WriteLine(DateTime.UtcNow.ToString("HH:mm:ss.ms") + "> reader is waiting ");


    //                //Exchange these 2 lines to make Reader event wait in async mode
    //                //ewh_Reader_ReadyToRead.WaitOne();
    //                await WaitHandleAsyncFactory.FromWaitHandle(ewh_Reader_ReadyToRead);//.ConfigureAwait(true);


    //                if (Interlocked.Read(ref this.sm.SharmIPC.Disposed) == 1)
    //                    return;

    //                //--STAT
    //                this.sm.SharmIPC.Statistic.Stop_WaitForRead_Signal();

    //                if (ewh_Reader_ReadyToRead == null) //Special Dispose case
    //                    return;                        

    //                //--STAT
    //                this.sm.SharmIPC.Statistic.Start_ReadProcedure_Signal();

    //                //Setting STOP for ewh_Reader_ReadyToRead.WaitOne()
    //                ewh_Reader_ReadyToRead.Reset();

    //                //Reading data from MMF

    //                //Reading header
    //                hdr = ReadBytes(1, 2);
    //                qMsg = BitConverter.ToUInt16(hdr, 0);                     
    //            }
    //            else
    //            {
    //                //First income from previous protocol
    //                //getting quantity of records 
    //                qMsg = BitConverter.ToUInt16(uhdr, 1); //2 bytes will tell quantity of messages                        
    //                //Cleaning uhdr, from the next iterration will work only previous loop
    //                uhdr = null;

    //            }

    //            for (int z=0;z< qMsg; z++)
    //            {
    //                hdr = ReadBytes(msgOffset, protocolLen);

    //                msgType = (eMsgType)hdr[0];
    //                //Parsing header
    //                switch (msgType)
    //                {
    //                    case eMsgType.ErrorInRpc:
    //                        jPos = 5;
    //                        iPayLoadLen = BitConverter.ToInt32(hdr, 9); //+4
    //                        iResponseMsgId = BitConverter.ToUInt64(hdr, 17); //+8

    //                        this.sm.SharmIPC.InternalDataArrived(msgType, iResponseMsgId, null);
    //                        jPos = 6;
    //                        break;

    //                    case eMsgType.RpcResponse:
    //                    case eMsgType.RpcRequest:
    //                    case eMsgType.Request:

    //                        jPos = 7;
    //                        bool zeroByte = false;
    //                        iMsgId = BitConverter.ToUInt64(hdr, 1); //+8
    //                        iPayLoadLen = BitConverter.ToInt32(hdr, 9); //+4
    //                        if (iPayLoadLen == Int32.MaxValue)
    //                        {
    //                            zeroByte = true;
    //                            iPayLoadLen = 0;
    //                        }
    //                        iCurChunk = BitConverter.ToUInt16(hdr, 13); //+2
    //                        iTotChunk = BitConverter.ToUInt16(hdr, 15); //+2     
    //                        iResponseMsgId = BitConverter.ToUInt64(hdr, 17); //+8
    //                        jPos = 8;
    //                        if (iCurChunk == 1)
    //                        {
    //                            chunksCollected = null;
    //                            MsgId_Received = iMsgId;
    //                        }
    //                        else if (iCurChunk != currentChunk + 1)
    //                        {
    //                            //Wrong income, sending special signal back, waiting for new MsgId   
    //                            switch (msgType)
    //                            {
    //                                case eMsgType.RpcRequest:
    //                                    jPos = 9;
    //                                    this.SendMessage(eMsgType.ErrorInRpc, this.GetMessageId(), null, iMsgId);
    //                                    jPos = 10;
    //                                    break;
    //                                case eMsgType.RpcResponse:
    //                                    jPos = 11;
    //                                    this.sm.SharmIPC.InternalDataArrived(eMsgType.ErrorInRpc, iResponseMsgId, null);
    //                                    jPos = 12;
    //                                    break;
    //                            }
    //                            break;
    //                        }

    //                        if (iTotChunk == iCurChunk)
    //                        {
    //                            jPos = 13;
    //                            if (chunksCollected == null)
    //                            {
    //                                jPos = 14;
    //                                //Was
    //                                //this.sm.SharmIPC.InternalDataArrived(msgType, (msgType == eMsgType.RpcResponse) ? iResponseMsgId : iMsgId, iPayLoadLen == 0 ? ((zeroByte) ? new byte[0] : null) : ReadBytes(Reader_accessor_ptr, protocolLen, iPayLoadLen));
    //                                jProtocolLen = protocolLen;
    //                                jPayloadLen = iPayLoadLen;
    //                                jReadBytes = ReadBytes(msgOffset + protocolLen, iPayLoadLen);
    //                                jPos = 27;
    //                                this.sm.SharmIPC.InternalDataArrived(msgType, (msgType == eMsgType.RpcResponse) ? iResponseMsgId : iMsgId, iPayLoadLen == 0 ? ((zeroByte) ? new byte[0] : null) : jReadBytes);
    //                                ///////////// test
    //                                jPos = 15;
    //                            }
    //                            else
    //                            {
    //                                jPos = 16;
    //                                ret = new byte[iPayLoadLen + chunksCollected.Length];
    //                                Buffer.BlockCopy(chunksCollected, 0, ret, 0, chunksCollected.Length);
    //                                Buffer.BlockCopy(ReadBytes(msgOffset + protocolLen, iPayLoadLen), 0, ret, chunksCollected.Length, iPayLoadLen);
    //                                this.sm.SharmIPC.InternalDataArrived(msgType, (msgType == eMsgType.RpcResponse) ? iResponseMsgId : iMsgId, ret);
    //                                jPos = 17;
    //                            }
    //                            chunksCollected = null;
    //                            currentChunk = 0;
    //                        }
    //                        else
    //                        {


    //                            jPos = 18;
    //                            if (chunksCollected == null)
    //                            {
    //                                jPos = 19;
    //                                chunksCollected = ReadBytes(msgOffset + protocolLen, iPayLoadLen);
    //                                jPos = 20;
    //                            }
    //                            else
    //                            {
    //                                jPos = 21;
    //                                byte[] tmp = new byte[chunksCollected.Length + iPayLoadLen];
    //                                Buffer.BlockCopy(chunksCollected, 0, tmp, 0, chunksCollected.Length);
    //                                Buffer.BlockCopy(ReadBytes(msgOffset + protocolLen, iPayLoadLen), 0, tmp, chunksCollected.Length, iPayLoadLen);
    //                                chunksCollected = tmp;
    //                                jPos = 22;
    //                            }
    //                            jPos = 23;
    //                            currentChunk = iCurChunk;
    //                        }
    //                        break;
    //                    default:
    //                        //Unknown protocol type
    //                        jPos = 24;
    //                        chunksCollected = null;
    //                        currentChunk = 0;
    //                        //Wrong income, doing nothing
    //                        throw new Exception("tiesky.com.SharmIpc: Reading protocol contains errors");
    //                        //break;
    //                }//eo switch

    //                msgOffset += protocolLen + iPayLoadLen;
    //            }//eo for loop of messages

    //            //Setting signal 
    //            ewh_Reader_ReadyToWrite.Set();

    //            //--STAT
    //            this.sm.SharmIPC.Statistic.Stop_ReadProcedure_Signal();

    //            //--STAT
    //            this.sm.SharmIPC.Statistic.Start_WaitForRead_Signal();                    
    //        }
    //    }
    //    catch (System.Exception ex)
    //    {             
    //        //constrained execution region (CER)
    //        //https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.safehandle.dangerousaddref(v=vs.110).aspx

    //        this.sm.SharmIPC.LogException("SharmIps.ReaderWriterHandler.InitReader LE, jPos=" + jPos + "; jProtLen=" + jProtocolLen + "; jPaylLen=" + jPayloadLen + "; jReadBytesLen=" + (jReadBytes == null ? 0 : jReadBytes.Length), ex);
    //    }


    //}//EO ReaderV02
} //eo class 