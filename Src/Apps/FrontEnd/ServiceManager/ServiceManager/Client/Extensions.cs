using System;
using ServiceManager.Client.ViewModels.Models;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class Extensions
    {
        public static void PublishError(this IEventAggregator aggregator, Exception error)
            => PublishMessage(aggregator, new SnackbarErrorMessage(error.Message));

        public static void PublishError(this IEventAggregator aggregator, string message)
            => PublishMessage(aggregator, new SnackbarErrorMessage(message));

        public static void PublishWarnig(this IEventAggregator aggregator, string message)
            => PublishMessage(aggregator, new SnackbarWarningMessage(message));

        public static void PublishInfo(this IEventAggregator aggregator, string message)
            => PublishMessage(aggregator, new SnackbarInfoMessage(message));

        public static void PublishSuccess(this IEventAggregator aggregator, string message)
            => PublishMessage(aggregator, new SnackbarSuccessMessage(message));

        public static void PublishNormal(this IEventAggregator aggregator, string message)
            => PublishMessage(aggregator, new SnackbarNormalMessage(message));

        public static void PublishMessage(this IEventAggregator aggregator, SnackbarMessage message)
            => aggregator.GetEvent<SharedEvent<SnackbarMessage>, SnackbarMessage>().Publish(message);

        public static IObservable<SnackbarMessage> ConsumeMessages(this IEventAggregator aggregator)
            => aggregator.GetEvent<SharedEvent<SnackbarMessage>, SnackbarMessage>().Get();
    }
}