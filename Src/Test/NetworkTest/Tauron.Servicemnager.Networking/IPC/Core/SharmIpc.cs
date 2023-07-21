using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Tauron.Servicemnager.Networking.IPC.Core;

/// <summary>
///     Inter-process communication handler. IPC for .NET
///     https://github.com/hhblaze/SharmIPC or http://sharmipc.tiesky.com
/// </summary>
[PublicAPI]
public class SharmIpc : IDisposable
{
    /// <summary>
    ///     Both peers must have the same version implementation
    /// </summary>
    public enum EProtocolVersion
    {
        V1 = 0, //Must be 0 for compatibility
        V2 = 2,
    }

    /// <summary>
    ///     If we don't want to answer in sync way via remoteCallHandler
    ///     msgId and data, msgId must be returned back with AsyncAnswerOnRemoteCall
    /// </summary>
    private readonly Action<ulong, byte[]?>? _asyncRemoteCallHandler;

    private readonly ConcurrentDictionary<ulong, ResponseCrate> _df = new();


    private readonly Action<string, Exception>? _externalExceptionHandler;

    /// <summary>
    ///     Default is false. Descibed in https://github.com/hhblaze/SharmIPC/issues/6
    ///     <para>Gives ability to parse packages in the same receiving thread before processing them in another thread</para>
    ///     <para>
    ///         Programmer is responsible for the returning control back ASAP from RemoteCallHandler via
    ///         Task.Run(()=>process(msg))
    ///     </para>
    /// </summary>
    private readonly bool _externalProcessing;

    private readonly Func<byte[]?, (bool ResponseOk, byte[]? Data)>? _remoteCallHandler;

    internal readonly Statistic Statistic = new();

    private SharedMemory _sm;

    /// <summary>
    ///     Removing timeout requests
    /// </summary>
    private Timer _tmr;

    internal long Disposed;

    /// <summary>
    ///     SharmIpc constructor
    /// </summary>
    /// <param name="uniqueHandlerName">Must be unique in OS scope (can be PID [ID of the process] + other identifications)</param>
    /// <param name="remoteCallHandler">Response routine for the remote partner requests</param>
    /// <param name="bufferCapacity">bigger buffer sends larger datablocks faster. Default value is 50000</param>
    /// <param name="maxQueueSizeInBytes">
    ///     If remote partner is temporary not available, messages are accumulated in the sending
    ///     buffer. This value sets the upper threshold of the buffer in bytes.
    /// </param>
    /// <param name="externalExceptionHandler">
    ///     External exception handler can be supplied, will be returned Description from
    ///     SharmIPC, like class.method name and handeled exception
    /// </param>
    /// <param name="protocolVersion">Version of communication protocol. Must be the same for both communicating peers</param>
    /// <param name="externalProcessing">
    ///     Gives ability to parse packages in the same receiving thread before processing them in
    ///     another thread
    /// </param>
    [PublicAPI]
    public SharmIpc(
        string uniqueHandlerName, Func<byte[]?, (bool ResponseOk, byte[]? Data)> remoteCallHandler, long bufferCapacity = 50000, int maxQueueSizeInBytes = 20000000,
        Action<string, Exception>? externalExceptionHandler = null, EProtocolVersion protocolVersion = EProtocolVersion.V1, bool externalProcessing = false)
        : this(uniqueHandlerName, bufferCapacity, maxQueueSizeInBytes, externalExceptionHandler, protocolVersion, externalProcessing)
        => _remoteCallHandler = remoteCallHandler ?? throw new InvalidOperationException("tiesky.com.SharmIpc: remoteCallHandler can't be null");

    /// <summary>
    ///     SharmIpc constructor
    /// </summary>
    /// <param name="uniqueHandlerName">Must be unique in OS scope (can be PID [ID of the process] + other identifications)</param>
    /// <param name="remoteCallHandler">
    ///     Callback routine for the remote partner requests. AsyncAnswerOnRemoteCall must be used
    ///     for answer
    /// </param>
    /// <param name="bufferCapacity">bigger buffer sends larger datablocks faster. Default value is 50000</param>
    /// <param name="maxQueueSizeInBytes">
    ///     If remote partner is temporary not available, messages are accumulated in the sending
    ///     buffer. This value sets the upper threshold of the buffer in bytes.
    /// </param>
    /// <param name="externalExceptionHandler">
    ///     External exception handler can be supplied, will be returned Description from
    ///     SharmIPC, like class.method name and handeled exception
    /// </param>
    /// <param name="protocolVersion">Version of communication protocol. Must be the same for both communicating peers</param>
    /// <param name="externalProcessing">
    ///     Gives ability to parse packages in the same receiving thread before processing them in
    ///     another thread
    /// </param>
    public SharmIpc(
        string uniqueHandlerName, Action<ulong, byte[]?> remoteCallHandler, long bufferCapacity = 50000, int maxQueueSizeInBytes = 20000000,
        Action<string, Exception>? externalExceptionHandler = null, EProtocolVersion protocolVersion = EProtocolVersion.V1, bool externalProcessing = false)
        : this(uniqueHandlerName, bufferCapacity, maxQueueSizeInBytes, externalExceptionHandler, protocolVersion, externalProcessing)
        => _asyncRemoteCallHandler = remoteCallHandler ?? throw new InvalidOperationException("tiesky.com.SharmIpc: remoteCallHandler can't be null");


    private SharmIpc(
        string uniqueHandlerName, long bufferCapacity = 50000, int maxQueueSizeInBytes = 20000000, Action<string, Exception>? externalExceptionHandler = null,
        EProtocolVersion protocolVersion = EProtocolVersion.V1, bool externalProcessing = false)
    {
        Statistic.Ipc = this;
        _externalProcessing = externalProcessing;

        _tmr = new Timer(
            _ =>
            {
                DateTime now = DateTime.UtcNow;

                //This timer is necessary for Calls based on Callbacks, calls based on WaitHandler have their own timeout,
                //That's why for non-callback calls, timeout will be infinite
                var toRemove = new List<ulong>();

                //foreach (var el in df.Where(r => now.Subtract(r.Value.created).TotalMilliseconds >= r.Value.TimeoutsMs))
                foreach (var el in _df.Where(r => now.Subtract(r.Value.Created).TotalMilliseconds >= r.Value.TimeoutsMs).ToList())
                    if(el.Value.CallBack != null)
                        toRemove.Add(el.Key);
                    else
                        el.Value.Set_MRE();

                foreach (ulong el in toRemove)
                    if(_df.TryRemove(el, out ResponseCrate? rc))
                        rc.CallBack?.Invoke((false, null)); //timeout

            },
            state: null,
            10000,
            10000);


        _externalExceptionHandler = externalExceptionHandler;
        _sm = new SharedMemory(uniqueHandlerName, this, bufferCapacity, maxQueueSizeInBytes, protocolVersion);
    }

    /// <summary>
    ///     Communication protocol, must be the same for both peers. Can be setup via constructor
    /// </summary>
    public EProtocolVersion ProtocolVersion => _sm.ProtocolVersion;

    /// <summary>
    /// </summary>
    public void Dispose()
    {
        if(Interlocked.CompareExchange(ref Disposed, 1, 0) != 0)
            return;


        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _tmr?.Dispose();
        _tmr = null!;


        foreach (var el in _df.ToList())
        {
            if(!_df.TryRemove(el.Key, out ResponseCrate? rc))
                continue;

            rc.IsRespOk = false;
            rc.Dispose_MRE();
        }


        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _sm?.Dispose();
        _sm = null!;
    }

    /// <summary>
    ///     Internal exception logger
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    internal void LogException(string description, Exception ex)
    {
        _externalExceptionHandler?.Invoke(description, ex);
    }

    /// <summary>
    ///     In case if asyncRemoteCallHandler != null
    /// </summary>
    /// <param name="msgId"></param>
    /// <param name="res"></param>
    public void AsyncAnswerOnRemoteCall(ulong msgId, (bool, byte[]?) res)
        => _sm.SendMessage(res.Item1 ? EMsgType.RpcResponse : EMsgType.ErrorInRpc, _sm.GetMessageId(), res.Item2, msgId);

    //async Task CallAsyncRemoteHandler(ulong msgId, byte[] bt)
    //{ 
    //    AsyncRemoteCallHandler(msgId, bt);
    //}

    /// <summary>
    ///     Any incoming data from remote partner is accumulated here
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msgId"></param>
    /// <param name="bt"></param>
    // ReSharper disable once CognitiveComplexity
    #pragma warning disable MA0051
    internal void InternalDataArrived(EMsgType msgType, ulong msgId, byte[]? bt)
        #pragma warning restore MA0051
    {

        switch (msgType)
        {
            case EMsgType.Request:

                if(_externalProcessing)
                {
                    if(_asyncRemoteCallHandler != null)
                        //CallAsyncRemoteHandler(msgId, bt);
                        _asyncRemoteCallHandler(msgId, bt);
                    //Answer must be supplied via AsyncAnswerOnRemoteCall
                    else
                        _remoteCallHandler?.Invoke(bt);
                }
                else
                {
                    #pragma warning disable EPC13
#pragma warning disable MA0134
                    Task.Run(
                        () =>
                        {
                            if(_asyncRemoteCallHandler != null)
                                //CallAsyncRemoteHandler(msgId, bt);
                                _asyncRemoteCallHandler(msgId, bt);
                            //Answer must be supplied via AsyncAnswerOnRemoteCall
                            else
                                _remoteCallHandler?.Invoke(bt);
                        });
                }

                break;
            case EMsgType.RpcRequest:

                Task.Run(
                    () =>
                    {
                        if(_asyncRemoteCallHandler != null)
                        {
                            _asyncRemoteCallHandler(msgId, bt);
                            //Answer must be supplied via AsyncAnswerOnRemoteCall
                        }
                        else
                        {
                            var resNull = _remoteCallHandler?.Invoke(bt);
                            var res = resNull.GetValueOrDefault();
                            _sm.SendMessage(res.ResponseOk ? EMsgType.RpcResponse : EMsgType.ErrorInRpc, _sm.GetMessageId(), res.Item2, msgId);
                        }
                    });

                break;
            case EMsgType.ErrorInRpc:
            case EMsgType.RpcResponse:

                if(_df.TryGetValue(msgId, out ResponseCrate? rsp))
                {
                    rsp.Res = bt;
                    rsp.IsRespOk = msgType == EMsgType.RpcResponse;

                    if(rsp.CallBack is null)
                    {

                        //rsp.mre.Set();  //Signalling, to make waiting in parallel thread to proceed
                        rsp.Set_MRE();
                    }
                    else
                    {
                        _df.TryRemove(msgId, out rsp);
                        //Calling callback in parallel thread, quicly to return to ReaderWriterhandler.Reader procedure
                        Task.Run(() => { rsp?.CallBack?.Invoke((rsp.IsRespOk, bt)); });
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(msgType), msgType, "Invalid message Type");
            #pragma warning restore EPC13
        }
    }

    //internal void InternalDataArrived(eMsgType msgType, ulong msgId, byte[] bt)
    //{
    //    ResponseCrate rsp = null;

    //    switch (msgType)
    //    {
    //        case eMsgType.Request:


    //            if (AsyncRemoteCallHandler != null)
    //            {
    //                RunAsync(msgId, bt);
    //            }
    //            else
    //            {
    //                RunV1(bt);

    //            }

    //            break;
    //        case eMsgType.RpcRequest:

    //            if (AsyncRemoteCallHandler != null)
    //            {
    //                RunAsync(msgId, bt);
    //                //Answer must be supplied via AsyncAnswerOnRemoteCall
    //            }
    //            else
    //            {
    //                Run(msgId, bt);
    //            }


    //            break;
    //        case eMsgType.ErrorInRpc:
    //        case eMsgType.RpcResponse:

    //            if (df.TryGetValue(msgId, out rsp))
    //            {
    //                rsp.res = bt;
    //                rsp.IsRespOk = msgType == eMsgType.RpcResponse;

    //                if (rsp.callBack == null)
    //                {

    //                    //rsp.mre.Set();  //Signalling, to make waiting in parallel thread to proceed
    //                    rsp.Set_MRE();
    //                }
    //                else
    //                {
    //                    df.TryRemove(msgId, out rsp);
    //                    //Calling callback in parallel thread, quicly to return to ReaderWriterhandler.Reader procedure
    //                    RunV2(rsp, bt);

    //                }
    //            }

    //            break;
    //    }
    //}

    //async Task RunAsync(ulong msgId, byte[] bt)
    //{
    //    AsyncRemoteCallHandler(msgId, bt);
    //}

    //async Task Run(ulong msgId, byte[] bt)
    //{
    //    var res = this.remoteCallHandler(bt);
    //    sm.SendMessage(res.Item1 ? eMsgType.RpcResponse : eMsgType.ErrorInRpc, sm.GetMessageId(), res.Item2, msgId);
    //}

    //async Task RunV1(byte[] bt)
    //{
    //    this.remoteCallHandler(bt);
    //}

    //async Task RunV2(ResponseCrate rsp, byte[] bt)
    //{
    //    rsp.callBack(new Tuple<bool, byte[]>(rsp.IsRespOk, bt));
    //}


    /// <summary>
    /// </summary>
    /// <param name="args">payload which must be send to remote partner</param>
    /// <param name="callBack">
    ///     if specified then response for the request will be returned into callBack (async). Default is
    ///     sync.
    /// </param>
    /// <param name="timeoutMs">Default 30 sec</param>
    /// <returns></returns>
    #pragma warning disable MA0051
    public (bool ResponseOk, byte[]? Data) RemoteRequest(byte[] args, Action<(bool ResponseOk, byte[]? Data)>? callBack = null, int timeoutMs = 30000)
        #pragma warning restore MA0051
    {

        ulong msgId = _sm.GetMessageId();
        var resp = new ResponseCrate();


        if(callBack != null)
        {
            resp.TimeoutsMs = timeoutMs; //IS NECESSARY FOR THE CALLBACK TYPE OF RETURN

            //Async return
            resp.CallBack = callBack;
            _df[msgId] = resp;
            if(!_sm.SendMessage(EMsgType.RpcRequest, msgId, args))
            {
                _df.TryRemove(msgId, out resp);
                callBack((false, null));

                return (false, null);
            }

            return (true, null);
        }

        resp.TimeoutsMs = int.MaxValue; //using timeout of the wait handle (not the timer)


        //resp.mre = new ManualResetEvent(false);
        resp.Init_MRE();

        _df[msgId] = resp;

        if(!_sm.SendMessage(EMsgType.RpcRequest, msgId, args))
        {
            resp.Dispose_MRE();
            //if (resp.mre != null)
            //    resp.mre.Dispose();
            //resp.mre = null;
            _df.TryRemove(msgId, out resp);

            return (false, null);
        }
        //else if (!resp.mre.WaitOne(timeoutMs))

        if(!resp.WaitOne_MRE(timeoutMs))
        {
            //--STAT
            Statistic.Timeout();

            //if (resp.mre != null)
            //    resp.mre.Dispose();
            //resp.mre = null;
            resp.Dispose_MRE();
            _df.TryRemove(msgId, out resp);

            return (false, null);
        }

        //if (resp.mre != null)
        //    resp.mre.Dispose();
        //resp.mre = null;
        resp.Dispose_MRE();

        return _df.TryRemove(msgId, out resp) ? (resp.IsRespOk, resp.Res) : (false, null);

    }


    /// <summary>
    ///     Usage var x = await RemoteRequestAsync(...);
    /// </summary>
    /// <param name="args">payload which must be send to remote partner</param>
    /// <param name="timeoutMs">Default 30 sec</param>
    /// <returns></returns>
    public async Task<(bool ResponseOk, byte[]? Data)> RemoteRequestAsync(byte[] args, int timeoutMs = 30000)
    {
        if(Interlocked.Read(ref Disposed) == 1)
            return (false, null);

        ulong msgId = _sm.GetMessageId();
        var resp = new ResponseCrate
                   {
                       TimeoutsMs = timeoutMs, //enable for amre
                   };
        //resp.TimeoutsMs = Int32.MaxValue; //using timeout of the wait handle (not the timer), enable for mre

        //resp.Init_MRE();
        resp.Init_AMRE();


        _df[msgId] = resp;

        if(!_sm.SendMessage(EMsgType.RpcRequest, msgId, args))
        {
            resp.Dispose_MRE();
            //if (resp.mre != null)
            //    resp.mre.Dispose();
            //resp.mre = null;
            _df.TryRemove(msgId, out resp);

            return (false, null);
        }

        //await resp.mre.AsTask(TimeSpan.FromMilliseconds(timeoutMs));        //enable for mre
        await (resp.Amre?.WaitAsync() ?? Task.CompletedTask).ConfigureAwait(false); //enable for amre


        resp.Dispose_MRE();

        return _df.TryRemove(msgId, out resp) ? (resp.IsRespOk, resp.Res) : (false, null);

    }


    /// <summary>
    ///     Just sends payload to remote partner without awaiting response from it.
    /// </summary>
    /// <param name="args">payload</param>
    /// <returns>if Message was accepted for sending</returns>
    public bool RemoteRequestWithoutResponse(byte[]? args)
    {
        if(Interlocked.Read(ref Disposed) == 1)
            return false;

        ulong msgId = _sm.GetMessageId();

        return _sm.SendMessage(EMsgType.Request, msgId, args);
    }

    /// <summary>
    ///     Returns current usage statistic
    /// </summary>
    /// <returns></returns>
    public string UsageReport()
        => Statistic.Report();

    private class ResponseCrate
    {
        internal readonly DateTime Created = DateTime.UtcNow;

        //async public Task<bool> WaitOneAsync()
        //{
        //    //if (Interlocked.Read(ref IsDisposed) == 1 || amre == null)
        //    //    return false;

        //    return await amre.WaitAsync();

        //}


        private long _isDisposed;

        /// <summary>
        ///     Not SLIM version must be used (it works faster for longer delay which RPCs are)
        /// </summary>
        private ManualResetEvent? _mre;

        internal AsyncManualResetEvent? Amre;
        internal Action<(bool ResponseOk, byte[]? Data)>? CallBack;
        internal bool IsRespOk;

        internal byte[]? Res;
        internal int TimeoutsMs = 30000;

        internal void Init_MRE()
            => _mre = new ManualResetEvent(initialState: false);

        /// <summary>
        ///     Works faster with timer than WaitOneAsync
        /// </summary>
        internal void Init_AMRE()
            => Amre = new AsyncManualResetEvent();

        internal void Set_MRE()
        {

            if(_mre != null)
                _mre.Set();
            else
                Amre?.Set();

        }

        internal bool WaitOne_MRE(int timeouts)
            //if (Interlocked.Read(ref IsDisposed) == 1 || mre == null)  //No sense
            //    return false;
            => _mre?.WaitOne(timeouts) ?? false;

        internal void Dispose_MRE()
        {
            if(Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
                return;

            if(_mre != null)
            {
                _mre.Set();
                _mre.Dispose();
                _mre = null;
            }
            else if(Amre != null)
            {
                Amre.Set();
                Amre = null;
            }
        }
    }
}