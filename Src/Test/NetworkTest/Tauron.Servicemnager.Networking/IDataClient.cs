using System.Threading.Channels;
using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking;

public interface IDataClient : IDisposable
{
    Task Run(CancellationToken token);

    void Close();

    ChannelReader<NetworkMessage> OnMessageReceived { get; }
    bool Send(NetworkMessage msg);
}