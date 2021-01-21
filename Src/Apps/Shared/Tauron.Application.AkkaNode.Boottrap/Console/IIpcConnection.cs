using System;

namespace Tauron.Application.AkkaNode.Bootstrap.Console
{
    public interface IIpcConnection
    {
        bool IsReady { get; }

        IObservable<TType> OnMessage<TType>();

        bool SendMessage<TMessage>(string to, TMessage message);
    }
}