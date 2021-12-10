using System;
using System.IO;
using Servicemnager.Networking.Data;

namespace Servicemnager.Networking.Transmitter;

public sealed class Reciever : IDisposable
{
    private readonly IDataClient _client;
    private readonly Func<Stream> _target;

    private Stream? _stream;

    public Reciever(Func<Stream> target, IDataClient client)
    {
        _target = target;
        _client = client;
    }

    public void Dispose() => _stream?.Dispose();

    public bool ProcessMessage(NetworkMessage msg)
    {
        try
        {
            switch (msg.Type)
            {
                case NetworkOperation.Deny:
                    #pragma warning disable EX006
                    throw new InvalidOperationException("Operation Cancelled from Server");
                case NetworkOperation.DataAccept:
                    _stream = _target();
                    _client.Send(NetworkMessage.Create(NetworkOperation.DataNext));

                    return true;
                case NetworkOperation.DataCompled:
                    return false;
                case NetworkOperation.DataChunk:
                    if (_stream is null)
                        throw new InvalidOperationException("Write Stream is null");
                    #pragma warning restore EX006
                    _stream.Write(msg.Data, 0, msg.RealLength);
                    _client.Send(NetworkMessage.Create(NetworkOperation.DataNext));

                    return true;
            }

            return false;
        }
        catch (Exception)
        {
            _client.Send(NetworkMessage.Create(NetworkOperation.Deny));

            throw;
        }
    }
}