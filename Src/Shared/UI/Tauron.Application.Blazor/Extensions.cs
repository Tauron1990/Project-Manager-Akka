﻿using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using ReactiveUI;
using Stl.Fusion;
using Tauron.Operations;

namespace Tauron.Application.Blazor;

[PublicAPI]
public static class Extensions
{
    public static bool IsLoading<TData>(this IState<TData> state)
        => state.Computed.ConsistencyState != ConsistencyState.Consistent;

    public static Func<TInput, bool> IsSuccess<TInput>(this IEventAggregator aggregator, Func<TInput, SimpleResult> runner)
    {
        return input =>
               {
                   try
                   {
                       SimpleResult result = runner(input);

                       if(result.IsSuccess())
                           return true;

                       aggregator.PublishWarnig(result.GetErrorString());

                       return false;
                   }
                   catch (Exception e)
                   {
                       aggregator.PublishError(e);

                       return false;
                   }
               };
    }

    public static async ValueTask<bool> IsSuccess(this IEventAggregator aggregator, Func<ValueTask<SimpleResult>> runner)
    {
        try
        {
            SimpleResult result = await runner().ConfigureAwait(false);

            if(result.IsSuccess())
                return true;

            aggregator.PublishWarnig(result.GetErrorString());

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
            await runner().ConfigureAwait(false);

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
            await runner().ConfigureAwait(false);

            return true;
        }
        catch (Exception e)
        {
            aggregator.PublishError(e);

            return false;
        }
    }

    public static void ReplaceState<TData>(this IMutableState<TData> state, Func<TData, TData> update)
        => state.Set(update(state.LastNonErrorValue));

    public static async Task ReplaceState<TData>(this IMutableState<TData> state, Func<TData, Task<TData>> update)
        => state.Set(await update(state.LastNonErrorValue).ConfigureAwait(false));

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