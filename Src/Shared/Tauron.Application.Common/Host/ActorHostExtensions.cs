using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    [PublicAPI]
    public static class ActorHostExtensions
    {
        public static IActorApplicationBuilder AddModule<TModule>(this IActorApplicationBuilder builder)
            where TModule : Module, new()
            => builder.ConfigureAutoFac(cb => cb.RegisterModule<TModule>());

        public static IActorApplicationBuilder UseContentRoot(this IActorApplicationBuilder hostBuilder, string contentRoot)
            => hostBuilder.Configuration(configBuilder 
                                             => configBuilder
                                            .AddInMemoryCollection(new[]
                                                                   {
                                                                       new KeyValuePair<string, string>(HostDefaults.ContentRootKey, contentRoot
                                                                                                                                  ?? throw new ArgumentNullException(nameof(contentRoot)))
                                                                   }));
    }
}