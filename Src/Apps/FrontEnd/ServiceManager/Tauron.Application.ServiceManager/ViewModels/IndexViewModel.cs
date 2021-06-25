using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using Autofac;
using DynamicData;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.ServiceManager.AppCore.ClusterTracking;
using Tauron.Application.ServiceManager.PageData;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class IndexViewModel : UiActor
    {
        public UIProperty<ImmutableDictionary<string, ClusterNodeInfo>> NodeInfos { get; }

        public IndexViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, IClusterNodeManager nodeManager) 
            : base(lifetimeScope, dispatcher)
        {
            NodeInfos = RegisterProperty<ImmutableDictionary<string, ClusterNodeInfo>>(nameof(NodeInfos))
               .WithDefaultValue(ImmutableDictionary<string, ClusterNodeInfo>.Empty);

            nodeManager.GetMemberChangeSet(new GetAllMemberChangeset()).PipeTo(Self);

            Receive<MemberChangeSet>(obs => obs.Subscribe(mcs =>
                                                          {
                                                              mcs.Observable
                                                                 .Synchronize(NodeInfos)
                                                                 .ForEachChange(c =>
                                                                                {
                                                                                    string url = c.Current.Member.Address.ToString();
                                                                                    ClusterNodeInfo targetInfo;

                                                                                    switch (c)
                                                                                    {
                                                                                        case { Reason: ChangeReason.Add }:
                                                                                            targetInfo = new ClusterNodeInfo(c.Current.Member.Address.ToString(), c.Current);
                                                                                            NodeInfos.Set(NodeInfos.Value.SetItem(url, targetInfo));
                                                                                            return;
                                                                                        case { Reason: ChangeReason.Remove }:
                                                                                            NodeInfos.Set(NodeInfos.Value.Remove(url));
                                                                                            return;
                                                                                        default:
                                                                                            if (NodeInfos.Value.TryGetValue(url, out targetInfo!))
                                                                                                break;
                                                                                            return;
                                                                                    }

                                                                                    targetInfo.Update(c.Current);
                                                                                })
                                                                 .Subscribe()
                                                                 .DisposeWith(this);
                                                          }));
        }
    }
}