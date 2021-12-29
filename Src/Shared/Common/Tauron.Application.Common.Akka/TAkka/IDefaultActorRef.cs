﻿using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
// ReSharper disable once UnusedTypeParameter
public interface IDefaultActorRef<TActor> : IInitableActorRef { }

[PublicAPI]
public sealed class EmptyActor<TActor> : IDefaultActorRef<TActor>
{
    public IActorRef Actor { get; } = ActorRefs.Nobody;

    public void Init(string? name = null) { }

    public void Init(IActorRefFactory factory, string? name = null) { }
}