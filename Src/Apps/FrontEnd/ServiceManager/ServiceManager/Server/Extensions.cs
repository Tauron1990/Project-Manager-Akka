using System;
using System.IO;
using System.Reflection;
using Autofac;
using ServiceManager.Server.AppCore.Helper;
using Tauron.Application;
using Tauron.Features;

namespace ServiceManager.Server
{
    public static class Extensions
    {
        public static void RegisterEventDispatcher<TEventType, TDispatcher, TDispatcherRef>(this ContainerBuilder builder, Delegate del) 
            where TDispatcherRef : IEventDispatcher 
            where TDispatcher : AggregateEvent<TEventType>, new()
        {
            builder.Register(c => c.Resolve<IEventAggregator>().GetEvent<TDispatcher, TEventType>()).SingleInstance();
            builder.RegisterFeature<TDispatcherRef, IEventDispatcher>(del);
        }
        
        public static string FileInAppDirectory(this string fileName)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Programm Datei nicht gefunden");

            return Path.Combine(basePath, fileName);
        }
    }
}