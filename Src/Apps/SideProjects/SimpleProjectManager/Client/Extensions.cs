using System.Net.Http.Json;
using System.Threading.Tasks.Sources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Stl.Fusion;
using Tauron.Application;

namespace SimpleProjectManager.Client
{
    public static class Extensions
    {
        public static bool IsLoading<TData>(this IState<TData> state)
            => state.Computed.ConsistencyState != ConsistencyState.Consistent;

        //public static async Task<bool> IsSuccess(this IEventAggregator aggregator, Func<Task<string>> runner)
        //{
        //    try
        //    {
        //        var result = await runner();

        //        if (string.IsNullOrWhiteSpace(result))
        //            return true;

        //        aggregator.PublishWarnig(result);

        //        return false;
        //    }
        //    catch (Exception e)
        //    {
        //        aggregator.PublishError(e);

        //        return false;
        //    }
        //}

        public static async ValueTask<bool> IsSuccess(this IEventAggregator aggregator, Func<ValueTask<string>> runner)
        {
            try
            {
                var result = await runner();

                if (string.IsNullOrWhiteSpace(result))
                    return true;

                aggregator.PublishWarnig(result);

                return false;
            }
            catch (Exception e)
            {
                aggregator.PublishError(e);

                return false;
            }
        }

        public static void ReplaceState<TData>(this IMutableState<TData> state, Func<TData, TData> update)
            => state.Set(update(state.LatestNonErrorValue));

        public static async Task ReplaceState<TData>(this IMutableState<TData> state, Func<TData, Task<TData>> update)
            => state.Set(await update(state.LatestNonErrorValue));

        public static void PublishError(this IEventAggregator aggregator, Exception error)
        {
            #if DEBUG
            Console.WriteLine(error);
            #endif
            PublishMessage(aggregator, new SnackbarErrorMessage($"{error.GetType().Name} -- {error.Message}"));
        }

        public static void PublishError(this IEventAggregator aggregator, string? message)
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
            => aggregator.GetEvent<AggregateEvent<SnackbarMessage>, SnackbarMessage>().Publish(message);

        public static IObservable<SnackbarMessage> ConsumeMessages(this IEventAggregator aggregator)
            => aggregator.GetEvent<AggregateEvent<SnackbarMessage>, SnackbarMessage>().Get();

        public static async Task<TResult?> PostJson<TData, TResult>(this HttpClient client, string url, TData data)
        {
            var response = await client.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResult>();
        }
        
    }

    public static class NavigationManagerExtensions
    {
        public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T? value)
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
            {
                if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
                {
                    value = (T)(object)valueAsInt;

                    return true;
                }

                if (typeof(T) == typeof(string))
                {
                    value = (T)(object)valueFromQueryString.ToString();

                    return true;
                }

                if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
                {
                    value = (T)(object)valueAsDecimal;

                    return true;
                }
            }

            value = default;

            return false;
        }
    }
}