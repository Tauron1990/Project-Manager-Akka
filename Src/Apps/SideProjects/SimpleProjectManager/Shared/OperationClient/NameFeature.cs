﻿using System.Reactive.Linq;

namespace SimpleProjectManager.Shared.OperationClient;

/*public sealed class NameFeature : ActorFeatureBase<NameFeature.State>
{
    public sealed record State(string Name);

    public static IPreparedFeature Create(string name)
        => Feature.Create(() => new NameFeature(), _ => new State(name));
    
    protected override void ConfigImpl()
    {
        Receive<NameRequest>(o => o.Select(s => s.NewEvent(new NameResponse(s.State.Name))).ToSender());
    }
}*/