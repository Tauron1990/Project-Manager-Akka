using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    [PublicAPI]
    public static class HostExtensions
    {
        public static IApplicationBuilder AddModule<TModule>(this IApplicationBuilder builder)
            where TModule : Module, new()
        {
            return builder.ConfigureAutoFac(cb => cb.RegisterModule<TModule>());
        }

        public static IApplicationBuilder UseContentRoot(this IApplicationBuilder hostBuilder, string contentRoot)
        {
            return hostBuilder.Configuration(configBuilder =>
            {
                configBuilder
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>(HostDefaults.ContentRootKey, contentRoot
                            ?? throw new ArgumentNullException(nameof(contentRoot)))
                    });
            });
        }
    }
}