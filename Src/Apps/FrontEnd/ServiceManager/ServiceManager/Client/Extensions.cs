using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared.Api;
using Tauron.Application;

namespace ServiceManager.Client
{
    public static class Extensions
    {
        public static void PublishError(this IEventAggregator aggregator, Exception error)
            => PublishMessage(aggregator, new SnackbarErrorMessage($"{error.GetType().Name} -- {error.Message}"));

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

        public static async Task<TResult?> PostJson<TData, TResult>(this HttpClient client, string url, TData data)
        {
            var response = await client.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResult>();
        }

        public static async Task<string> PostJsonDefaultError<TData>(this HttpClient client, string url, TData data)
        {
            var response = await client.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<StringApiContent>();

            return result == null
                ? "Unbekannter Fehler beim Update"
                : result.Content;
        }
    }
}