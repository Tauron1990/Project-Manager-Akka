using System;
using Akka.Actor;
using FluentResults;

namespace Tauron.Application.AkkaNode.Services.Reporting;

public interface IReporterMessage
{
    IActorRef Listner { get; }

    string Info { get; }

    void SetListner(IActorRef listner);

    Result ValidateApi(Type apiType);
}