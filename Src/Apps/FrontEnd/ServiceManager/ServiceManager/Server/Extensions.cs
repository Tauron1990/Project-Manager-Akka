using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Server.AppCore.Helper;
using Tauron.AkkaHost;
using Tauron.Application;
using Tauron.Features;

namespace ServiceManager.Server
{
    public static class Extensions
    {
        public static void RegisterEventDispatcher<TEventType, TDispatcher, TDispatcherRef>(this IActorApplicationBuilder builder, Delegate del, string name)
            where TDispatcherRef : class, IEventDispatcher, IFeatureActorRef<TDispatcherRef>
            where TDispatcher : AggregateEvent<TEventType>, new()
        {
            builder.ConfigureServices(
                s => s
                    .AddSingleton(c => c.GetRequiredService<IEventAggregator>().GetEvent<TDispatcher, TEventType>())
                    .AddTransient<IEventDispatcher>(sp => sp.GetRequiredService<TDispatcherRef>()));
            builder.RegisterFeature<TDispatcherRef>(del, name);
        }

        public static string FileInAppDirectory(this string fileName)
        {
            string? basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            if (string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Programm Datei nicht gefunden");

            return Path.Combine(basePath, fileName);
        }
    }
}