using System;
using System.IO;
using System.Threading;
using Servicemnager.Networking.Data;

namespace Servicemnager.Networking.Transmitter;

public sealed class Sender : IDisposable
{
    private readonly Client _client;
    private readonly Action<Exception> _errorHandler;
    private readonly Func<byte[]> _getArray;
    private readonly Action<byte[]> _returnArray;
    private readonly IDataServer _server;
    private readonly Stream _toSend;

    private bool _isRunnging;

    public Sender(
        Stream toSend, Client client, IDataServer server, Func<byte[]> getArray,
        Action<byte[]> returnArray, Action<Exception> errorHandler)
    {
        _toSend = toSend;
        _client = client;
        _server = server;
        _getArray = getArray;
        _returnArray = returnArray;
        _errorHandler = errorHandler;
    }

    public void Dispose() => _toSend.Dispose();

    public bool ProcessMessage(NetworkMessage msg)
    {
        try
        {
            if(!_isRunnging)
            {
                _server.Send(_client, NetworkMessage.Create(NetworkOperation.DataAccept.Value));
                _isRunnging = true;

                return true;
            }

            if(msg.Type == NetworkOperation.Deny)
            {
                _isRunnging = false;
                _toSend.Dispose();
                _errorHandler(new InvalidOperationException("Operation Cancelled from Client"));

                return false;
            }

            if(msg.Type == NetworkOperation.DataNext)
                return HandleNext();


            _toSend.Dispose();

            return false;
        }
        catch (Exception e)
        {
            _toSend.Dispose();
            _errorHandler(e);
            _server.Send(_client, NetworkMessage.Create(NetworkOperation.Deny.Value));

            return false;
        }
    }

    private bool HandleNext()
    {
        byte[] chunk = _getArray();
        try
        {
            int count = _toSend.Read(chunk, 0, chunk.Length);
            if(count == 0)
            {
                _toSend.Dispose();
                _server.Send(_client, NetworkMessage.Create(NetworkOperation.DataCompled.Value));
                Thread.Sleep(2000);

                return false;
            }

            _server.Send(_client, NetworkMessage.Create(NetworkOperation.DataChunk.Value, chunk, count));

            return true;
        }
        finally
        {
            _returnArray(chunk);
        }
    }
}