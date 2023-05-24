using System;
using System.Threading.Tasks;
using FluentResults;
using JetBrains.Annotations;
using Tauron.Servicemnager.Networking;

namespace Tauron.Application.AkkaNode.Bootstrap;

[PublicAPI]
public interface IIpcConnection
{
    bool IsReady { get; }

    IObservable<Result<TType>> OnMessage<TType>();

    ValueTask<Result> SendMessage<TMessage>(Client to, TMessage message);

    ValueTask<Result> SendMessage<TMessage>(TMessage message);
}