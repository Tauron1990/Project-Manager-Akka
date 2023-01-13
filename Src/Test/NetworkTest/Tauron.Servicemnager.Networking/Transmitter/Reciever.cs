using JetBrains.Annotations;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.Transmitter;

[PublicAPI]
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
            if(msg.Type == NetworkOperation.Deny)
            {
                #pragma warning disable EX006
                throw new InvalidOperationException("Operation Cancelled from Server");
            }

            if(msg.Type == NetworkOperation.DataAccept)
            {
                _stream = _target();
                _client.Send(NetworkMessage.Create(NetworkOperation.DataNext.Value));

                return true;
            }

            if(msg.Type == NetworkOperation.DataCompled)
                return false;

            if(msg.Type != NetworkOperation.DataChunk)
                return false;

            if(_stream is null)
                throw new InvalidOperationException("Write Stream is null");

            #pragma warning restore EX006
            _stream.Write(msg.Data, 0, msg.RealLength);
            _client.Send(NetworkMessage.Create(NetworkOperation.DataNext.Value));

            return true;
        }
        catch (Exception)
        {
            _client.Send(NetworkMessage.Create(NetworkOperation.Deny.Value));

            throw;
        }
    }
}