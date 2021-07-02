using System;
using ServiceManager.Client.AppCore;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class Extensions
    {
        public static void PublishError(this IEventAggregator aggregator, Exception error)
            => PublishMessage(aggregator, new SnackbarErrorMessage(error.Message));

        public static void PublishMessage(this IEventAggregator aggregator, SnackbarMessage message)
            => aggregator.GetEvent<SharedEvent<SnackbarMessage>, SnackbarMessage>().Publish(message);

        public static IObservable<SnackbarMessage> ConsumeMessages(this IEventAggregator aggregator)
            => aggregator.GetEvent<SharedEvent<SnackbarMessage>, SnackbarMessage>().Subscribe();
    }
}