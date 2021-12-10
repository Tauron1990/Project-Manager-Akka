using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public abstract record ReporterCommandBase<TSender, TThis> : IReporterMessage
    where TSender : ISender
    where TThis : ReporterCommandBase<TSender, TThis>
{
    private IActorRef _listner = ActorRefs.NoSender;

    protected abstract string Info { get; }

    [UsedImplicitly]
    public IActorRef Listner
    {
        get => _listner;
        set
        {
            if (!Listner.IsNobody())
                throw new InvalidOperationException("Only One Listner Can be Set");

            _listner = value;
        }
    }

    string IReporterMessage.Info => Info;

    public void ValidateApi(Type apiType)
    {
        if (apiType.IsAssignableTo(typeof(TSender)))
            return;

        throw new InvalidOperationException($"Incompatible Command Api Type: {apiType} Expeced: {typeof(TSender)}");
    }

    public void SetListner(IActorRef listner) => Listner = listner;
}