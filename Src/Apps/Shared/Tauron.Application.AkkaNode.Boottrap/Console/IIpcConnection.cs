using System;
using JetBrains.Annotations;
using Servicemnager.Networking;

namespace Tauron.Application.AkkaNode.Bootstrap;

[PublicAPI]
public interface IIpcConnection
{
    bool IsReady { get; }

    IObservable<CallResult<TType>> OnMessage<TType>();

    bool SendMessage<TMessage>(in Client to, TMessage message);

    bool SendMessage<TMessage>(TMessage message);
}