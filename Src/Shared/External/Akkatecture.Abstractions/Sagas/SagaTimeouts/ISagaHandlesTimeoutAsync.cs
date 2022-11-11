using JetBrains.Annotations;

namespace Akkatecture.Sagas.SagaTimeouts;

[PublicAPI]
public interface ISagaHandlesTimeoutAsync<in TTimeout> : ISaga
    where TTimeout : class, ISagaTimeoutJob
{
    Task HandleTimeoutAsync(TTimeout timeout);
}