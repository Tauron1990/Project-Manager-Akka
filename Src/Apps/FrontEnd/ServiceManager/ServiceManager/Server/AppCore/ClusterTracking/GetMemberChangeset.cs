using System;
using DynamicData;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public abstract record GetMemberChangeset
    {
        public abstract IObservable<IChangeSet<MemberData, MemberAddress>> Prepare(IObservable<IChangeSet<MemberData, MemberAddress>> from);
    }

    public sealed record GetAllMemberChangeset : GetMemberChangeset
    {
        public override IObservable<IChangeSet<MemberData, MemberAddress>> Prepare(IObservable<IChangeSet<MemberData, MemberAddress>> from) => from;
    }

    public sealed record GetMemberChangesetByServiceType(ServiceType ServiceType) : GetMemberChangeset
    {
        public override IObservable<IChangeSet<MemberData, MemberAddress>> Prepare(IObservable<IChangeSet<MemberData, MemberAddress>> from) 
            => @from.Filter(md => md.ServiceType == ServiceType);
    }

    public sealed record MemberChangeSet(IObservable<IChangeSet<MemberData, MemberAddress>> Observable);
}