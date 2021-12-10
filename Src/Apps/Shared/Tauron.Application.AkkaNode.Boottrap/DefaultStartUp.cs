using Akka.Actor;
using Akka.Cluster;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Bootstrap;

[PublicAPI]
public abstract class ClusterStartUp : IStartUpAction
{
    protected ClusterStartUp(ActorSystem system) => System = system;

    public ActorSystem System { get; }

    public Cluster Cluster => Cluster.Get(System);

    void IStartUpAction.Run()
    {
        Cluster.RegisterOnMemberUp(MemberUp);
        Cluster.RegisterOnMemberRemoved(MemberRemoved);
        Run();
    }

    public virtual void MemberUp() { }

    public virtual void MemberRemoved() { }

    public virtual void Run() { }
}