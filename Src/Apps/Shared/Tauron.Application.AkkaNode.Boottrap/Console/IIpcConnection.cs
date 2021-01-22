using System;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Bootstrap.Console
{
    [PublicAPI]
    public interface IIpcConnection
    {
        bool IsReady { get; }

        IObservable<CallResult<TType>> OnMessage<TType>();

        bool SendMessage<TMessage>(string to, TMessage message);

        bool SendMessage<TMessage>(TMessage message);
    }
}