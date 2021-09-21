namespace Stl.Fusion.AkkaBridge.Internal
{
    public interface IServiceAccessor
    {
        object Service { get; }
    }

    public interface IServiceAccessor<out TType> : IServiceAccessor
    {
        TType TypedService { get; }
    }

    public sealed class ServiceAccessor<TType> : IServiceAccessor<TType>
    {
        public ServiceAccessor(TType typedService)
            => TypedService = typedService;

        public object Service => TypedService!;
        public TType TypedService { get; }
    }
}