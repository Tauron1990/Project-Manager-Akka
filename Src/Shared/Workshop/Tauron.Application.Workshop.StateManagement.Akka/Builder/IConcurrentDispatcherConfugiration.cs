using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Akka.Builder;

[PublicAPI]
public interface
    IConcurrentDispatcherConfugiration : IDispatcherPoolConfiguration<IConcurrentDispatcherConfugiration> { }