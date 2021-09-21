using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion.AkkaBridge.Internal;

namespace Stl.Fusion.AkkaBridge
{
    public readonly struct PublisherBuilder
    {
        private readonly IServiceCollection _service;

        public PublisherBuilder(IServiceCollection service)
            => _service = service;

        public PublisherBuilder PublishService<TService>()
            where TService : class
        {
            _service.AddTransient(sp => new PublishService(typeof(TService), () => sp.GetRequiredService<TService>()));

            return this;
        }
    }
}