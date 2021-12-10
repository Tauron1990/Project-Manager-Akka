using System;
using Akka.Actor;

namespace Tauron.Application.AkkaNode.Services.Reporting;

public interface IReporterMessage
{
    IActorRef Listner { get; }

    string Info { get; }

    void SetListner(IActorRef listner);

    void ValidateApi(Type apiType);
}