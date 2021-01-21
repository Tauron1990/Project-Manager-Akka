using Servicemnager.Networking;

namespace Tauron.Application.AkkaNode.Bootstrap.Console.Core
{
    public interface IIpcRoot
    {
        bool IsReady { get; }

        bool IsMaster { get; }

        string ErrorMessage { get; }

        IDataClient Client();

        IDataServer Server();
    }
}