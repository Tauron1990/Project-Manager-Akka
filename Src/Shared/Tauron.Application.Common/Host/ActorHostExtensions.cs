using Autofac;
using JetBrains.Annotations;

namespace Tauron.Host
{
    [PublicAPI]
    public static class ActorHostExtensions
    {
        public static IActorApplicationBuilder AddModule<TModule>(this IActorApplicationBuilder builder)
            where TModule : Module, new()
            => builder.ConfigureAutoFac(cb => cb.RegisterModule<TModule>());
    }
}