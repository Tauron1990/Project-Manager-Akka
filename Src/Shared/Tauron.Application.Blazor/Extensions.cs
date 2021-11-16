using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Stl;
using Stl.Fusion;

namespace Tauron.Application.Blazor
{
    [PublicAPI]
    public static class Extensions
    {
        public static bool IsLoading<TData>(this IState<TData> state)
            => state.Computed.ConsistencyState != ConsistencyState.Consistent;

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

        public static IDisposableState<TData> ToState<TData>(this IObservable<TData> input, IStateFactory factory)
        {
            var serial = new SerialDisposable();
            var state = factory.NewMutable(new MutableState<TData>.Options());
            serial.Disposable = input.AutoSubscribe(n => state.Set(n), () => serial.Dispose(), e => state.Set(Result.Error<TData>(e)));

            return new DisposableState<TData>(state, serial);
        }

        public static IObservable<TData> ToObservable<TData>(this IState<TData> state)
            => Observable.Create<TData>(o =>
                                        {
                                            if(state.HasValue)
                                                o.OnNext(state.Value);
                                            return new StateRegistration<TData>(o, state);
                                        })
               .DistinctUntilChanged();
        
        private sealed class StateRegistration<TData> : IDisposable
        {
            private readonly IObserver<TData> _observer;
            private readonly IState<TData> _state;

            internal StateRegistration(IObserver<TData> observer, IState<TData> state)
            {
                _observer = observer;
                _state = state;
                
                state.AddEventHandler(StateEventKind.All, Handler);
            }

            private void Handler(IState<TData> arg1, StateEventKind arg2)
            {
                if(_state.HasValue)
                    _observer.OnNext(_state.Value);
                else if(_state.HasError && _state.Error is not null)
                    _observer.OnError(_state.Error);
            }

            public void Dispose() => _state.RemoveEventHandler(StateEventKind.All, Handler);
        }

        public static Scoped<TService> GetIsolatedService<TService>(this IServiceProvider serviceProvider) 
            where TService : notnull => new(serviceProvider);
    }

    [PublicAPI]
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