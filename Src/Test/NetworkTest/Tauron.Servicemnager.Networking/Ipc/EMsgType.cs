namespace Tauron.Servicemnager.Networking.Ipc;

internal enum EMsgType : byte
{
    RpcRequest = 1,
    RpcResponse = 2,
    ErrorInRpc = 3,
    Request = 4,

    //SwitchToV2=5
}