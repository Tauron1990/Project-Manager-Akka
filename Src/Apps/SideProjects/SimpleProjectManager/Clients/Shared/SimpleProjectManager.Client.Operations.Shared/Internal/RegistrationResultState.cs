using Akka.Actor;
using OneOf;

namespace SimpleProjectManager.Client.Operations.Shared.Internal;

[GenerateOneOf]
public sealed partial class RegistrationResult : OneOfBase<Success, Duplicate, Timeout, UnInitialized, Exception>
{
    public required IActorRef From { get; init; } = ActorRefs.Nobody;
}