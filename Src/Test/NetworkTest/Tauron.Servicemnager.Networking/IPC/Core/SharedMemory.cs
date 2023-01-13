namespace Tauron.Servicemnager.Networking.IPC.Core;

internal class SharedMemory : IDisposable
{
    internal readonly long BufferCapacity;
    internal readonly EInstanceType InstanceType;
    internal readonly int MaxQueueSizeInBytes;

    internal readonly SharmIpc.EProtocolVersion ProtocolVersion;
    internal readonly SharmIpc SharmIpc;

    //EventWaitHandle ewh_ReadyToRead = null;
    //EventWaitHandle ewh_ReadyToWrite = null;

    internal readonly string UniqueHandlerName;


    //System.IO.MemoryMappedFiles.MemoryMappedViewAccessor accessor = null;
    //System.IO.MemoryMappedFiles.MemoryMappedFile mmf = null;

    private Mutex? _mt;

    private ReaderWriterHandler _rwh;

    /// <summary>
    /// </summary>
    /// <param name="uniqueHandlerName">
    ///     Can be name of APP, both syncronized processes must use the same name and it must be
    ///     unique among the OS
    /// </param>
    /// <param name="sharmIpc">SharmIPC instance</param>
    /// <param name="bufferCapacity"></param>
    /// <param name="maxQueueSizeInBytes"></param>
    /// <param name="protocolVersion"></param>
    internal SharedMemory(
        string uniqueHandlerName,
        SharmIpc sharmIpc,
        long bufferCapacity = 50000,
        int maxQueueSizeInBytes = 20000000,
        SharmIpc.EProtocolVersion protocolVersion = SharmIpc.EProtocolVersion.V1)
    {
        SharmIpc = sharmIpc;
        MaxQueueSizeInBytes = maxQueueSizeInBytes;
        ProtocolVersion = protocolVersion;

        if(string.IsNullOrEmpty(uniqueHandlerName) || uniqueHandlerName.Length > 200)
            throw new InvalidOperationException("tiesky.com.SharmIpc: uniqueHandlerName can't be empty or more then 200 symbols");

        if(bufferCapacity < 256)
            bufferCapacity = 256;

        if(bufferCapacity > 1000000) //max 1MB
            bufferCapacity = 1000000;

        UniqueHandlerName = uniqueHandlerName;
        BufferCapacity = bufferCapacity;

        try
        {
            _mt = new Mutex(initiallyOwned: true, $"{uniqueHandlerName}SharmNet_MasterMutex");

            if(_mt.WaitOne(500))
            {
                InstanceType = EInstanceType.Master;
            }
            else
            {
                InstanceType = EInstanceType.Slave;
                if(_mt != null)
                {
                    //mt.ReleaseMutex();
                    _mt.Close();
                    _mt.Dispose();
                    _mt = null;
                }
            }
        }
        catch (AbandonedMutexException)
        {
            InstanceType = EInstanceType.Master;
        }

        _rwh = new ReaderWriterHandler(this);
    }

    /// <summary>
    ///     Disposing
    /// </summary>
    public void Dispose()
    {
        _mt?.ReleaseMutex();
        _mt?.Close();
        _mt?.Dispose();
        _mt = null;

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _rwh?.Dispose();
        _rwh = null!;
    }


    internal ulong GetMessageId()
        => _rwh.GetMessageId();

    internal bool SendMessage(EMsgType msgType, ulong msgId, byte[]? msg, ulong responseMsgId = 0)
    {
        switch (ProtocolVersion)
        {
            case SharmIpc.EProtocolVersion.V1:
                return _rwh.SendMessage(msgType, msgId, msg, responseMsgId);
            case SharmIpc.EProtocolVersion.V2:
                return _rwh.SendMessageV2(msgType, msgId, msg, responseMsgId);
            default:
                return false;
        }
    }


    //public void TestSendMessage()
    //{
    //    this.rwh.TestSendMessage();
    //}
} //eoc