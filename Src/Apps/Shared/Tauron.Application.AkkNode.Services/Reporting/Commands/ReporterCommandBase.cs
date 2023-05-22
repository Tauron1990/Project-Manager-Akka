using System;
using Akka.Actor;
using FluentResults;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public abstract record ReporterCommandBase<TSender, TThis> : IReporterMessage
    where TSender : ISender
    where TThis : ReporterCommandBase<TSender, TThis>
{
    protected abstract string Info { get; }

    [UsedImplicitly]
    public IActorRef Listner { get; set; } = ActorRefs.NoSender;

    string IReporterMessage.Info => Info;

    public Result ValidateApi(Type apiType)
    {
        return apiType.IsAssignableTo(typeof(TSender)) 
            ? Result.Ok() 
            : Result.Fail($"Incompatible Command Api Type: {apiType} Expeced: {typeof(TSender)}");

    }

    public void SetListner(IActorRef listner) => Listner = listner;
}