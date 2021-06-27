using System;
using System.IO;
using System.Reflection;
using Tauron.Application.ServiceManager.AppCore;

namespace Tauron.Application.ServiceManager
{
    public static class Extensions
    {
        public static void PublishError(this IEventAggregator aggregator, Exception error)
            => PublishMessage(aggregator, new SnackbarErrorMessage(error.Message));

        public static void PublishMessage(this IEventAggregator aggregator, SnackbarMessage message)
            => aggregator.GetEvent<SharedEvent<SnackbarMessage>, SnackbarMessage>().Publish(message);

        public static IObservable<SnackbarMessage> ConsumeMessages(this IEventAggregator aggregator)
            => aggregator.GetEvent<SharedEvent<SnackbarMessage>, SnackbarMessage>().Subscribe();

        public static string FileInAppDirectory(this string fileName)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Konfigurations Datei nicht gefunden");

            return Path.Combine(basePath, fileName);
        }
    }
}