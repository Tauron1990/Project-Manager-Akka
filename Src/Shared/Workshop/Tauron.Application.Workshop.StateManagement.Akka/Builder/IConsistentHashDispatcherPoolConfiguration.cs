using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Akka.Builder;

[PublicAPI]
public interface
    IConsistentHashDispatcherPoolConfiguration : IDispatcherPoolConfiguration<
        IConsistentHashDispatcherPoolConfiguration>
{
    public IConsistentHashDispatcherPoolConfiguration WithVirtualNodesFactor(int vnodes);

    public IConsistentHashDispatcherPoolConfiguration WithHashSelector(Func<object, object> selector);
}