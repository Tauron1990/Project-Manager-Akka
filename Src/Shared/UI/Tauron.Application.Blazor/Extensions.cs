using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using ReactiveUI;
using Stl.Fusion;

namespace Tauron.Application.Blazor;

[PublicAPI]
public static class Extensions
{
    public static bool IsLoading<TData>(this IState<TData> state)
        => state.Computed.ConsistencyState != ConsistencyState.Consistent;

    public static Func<TInput, bool> IsSuccess<TInput>(this IEventAggregator aggregator, Func<TInput, string> runner)
    {
        return input =>
               {
                   try
                   {
                       string result = runner(input);

                       if(string.IsNullOrWhiteSpace(result))
                           return true;

                       aggregator.PublishWarnig(result);

                       return false;
                   }
                   catch (Exception e)
                   {
                       aggregator.PublishError(e);

                       return false;
                   }
               };
    }

    public static async ValueTask<bool> IsSuccess(this IEventAggregator aggregator, Func<ValueTask<string>> runner)
    {
        try
        {
            string result = await runner();

            if(string.IsNullOrWhiteSpace(result))
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

    public static async ValueTask<bool> IsSuccess(this IEventAggregator aggregator, Func<ValueTask<Unit>> runner)
    {
        try
        {
            await runner();

            return true;
        }
        catch (Exception e)
        {
            aggregator.PublishError(e);

            return false;
        }
    }

    public static async ValueTask<bool> IsSuccess(this IEventAggregator aggregator, Func<ValueTask> runner)
    {
        try
        {
            await runner();

            return true;
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
        error = error.Demystify();

        #if DEBUG
        Console.WriteLine(error);
        #endif
        PublishMessage(aggregator, new SnackbarErrorMessage($"{error.GetType().Name} -- {error.Message}"));
    }

    public static void PublishError(this IEventAggregator aggregator, string? message)
        => PublishMessage(aggregator, new SnackbarErrorMessage(message));

    public static void PublishWarnig(this IEventAggregator aggregator, string message)
        => PublishMessage(aggregator, new SnackbarWarningMessage(message));

    public static void PublishWarnig(this IEventAggregator aggregator, Exception error)
    {
        error = error.Demystify();

        #if DEBUG
        Console.WriteLine(error);
        #endif
        PublishMessage(aggregator, new SnackbarWarningMessage($"{error.GetType().Name} -- {error.Message}"));
    }

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

    public static Scoped<TService> GetIsolatedService<TService>(this IServiceProvider serviceProvider)
        where TService : notnull => new(serviceProvider);

    public static Action<TInput> ToAction<TInput, TResult>(this ReactiveCommand<TInput, TResult> command)
    {
        var com = (ICommand)command;

        return i =>
               {
                   if(com.CanExecute(i))
                       com.Execute(i);
               };
    }

    public static Action ToAction<TResult>(this ReactiveCommand<Unit, TResult> command)
    {
        var com = (ICommand)command;

        return () =>
               {
                   if(com.CanExecute(Unit.Default))
                       com.Execute(Unit.Default);
               };
    }
}

[PublicAPI]
public static class NavigationManagerExtensions
{
    public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T? value)
    {
        Uri uri = navManager.ToAbsoluteUri(navManager.Uri);

        if(QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out StringValues valueFromQueryString))
        {
            if(typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out int valueAsInt))
            {
                value = (T)(object)valueAsInt;

                return true;
            }

            if(typeof(T) == typeof(string))
            {
                value = (T)(object)valueFromQueryString.ToString();

                return true;
            }

            if(typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out decimal valueAsDecimal))
            {
                value = (T)(object)valueAsDecimal;

                return true;
            }
        }

        value = default;

        return false;
    }
}