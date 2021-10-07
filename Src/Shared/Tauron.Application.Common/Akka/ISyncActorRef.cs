using JetBrains.Annotations;

namespace Tauron.Akka;

[PublicAPI]
// ReSharper disable once UnusedTypeParameter
public interface ISyncActorRef<TActor> : IInitableActorRef { }