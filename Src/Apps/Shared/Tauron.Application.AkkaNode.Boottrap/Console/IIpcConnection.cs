using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Servicemnager.Networking;

namespace Tauron.Application.AkkaNode.Bootstrap;

[PublicAPI]
public interface IIpcConnection
{
    bool IsReady { get; }

    IObservable<CallResult<TType>> OnMessage<TType>();

    ValueTask<bool> SendMessage<TMessage>(Client to, TMessage message);

    ValueTask<bool> SendMessage<TMessage>(TMessage message);
}