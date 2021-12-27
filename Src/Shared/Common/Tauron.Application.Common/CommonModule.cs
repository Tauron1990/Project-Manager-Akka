using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.PlatformServices;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application;
using Tauron.Application.VirtualFiles;

namespace Tauron;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public sealed class CommonModule : IModule
{
    private sealed class Clock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.Now;
    }

    public void Load(IServiceCollection builder)
    {
        #pragma warning disable GU0011
        builder.AddTransient<VirtualFileFactory>();
        builder.AddTransient<ISystemClock, Clock>();

        builder.AddSingleton<ITauronEnviroment, TauronEnviromentImpl>();
        builder.AddSingleton<IEventAggregator, EventAggregator>();
    }
}