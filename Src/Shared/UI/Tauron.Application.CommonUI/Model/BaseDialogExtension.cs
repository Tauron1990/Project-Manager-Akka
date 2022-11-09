using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.TAkka;

namespace Tauron.Application.CommonUI.Model;

[PublicAPI]
public static class BaseDialogExtension
{
    private static IDialogCoordinator? _dialogCoordinator;

    public static Task<TData> ShowDialogAsync<TDialog, TData, TViewData>(this UiActor actor, Func<TViewData> initalData, params object[] parameters)
        where TDialog : IBaseDialog<TData, TViewData>
        => ShowDialog<TDialog, TData, TViewData>(actor, initalData, parameters)();

    public static Task<TData> ShowDialogAsync<TDialog, TData>(this UiActor actor, Func<TData> initalData, params object[] parameters)
        where TDialog : IBaseDialog<TData, TData>
        => ShowDialog<TDialog, TData>(actor, initalData, parameters)();

    public static Task<TData> ShowDialogAsync<TDialog, TData>(this UiActor actor, params object[] parameters)
        where TDialog : IBaseDialog<TData, TData>
        => ShowDialog<TDialog, TData>(actor, parameters)();

    public static Func<Task<TData>> ShowDialog<TDialog, TData>(this UiActor actor, params object[] parameters)
        where TDialog : IBaseDialog<TData, TData>
        => ShowDialog<TDialog, TData, TData>(actor, () => default!, parameters);

    //public static Func<Task<TData>> ShowDialog<TDialog, TData>(this UiActor actor, params Parameter[] parameters)
    //    where TDialog : IBaseDialog<TData, TData>
    //{
    //    return ShowDialog<TDialog, TData, TData>(actor, () => default!, parameters);
    //}

    public static Func<Task<TData>> ShowDialog<TDialog, TData>(
        this UiActor actor, Func<TData> initalData,
        params object[] parameters)
        where TDialog : IBaseDialog<TData, TData>
        => ShowDialog<TDialog, TData, TData>(actor, initalData, parameters);

    public static Func<Task<TData>> ShowDialog<TDialog, TData, TViewData>(
        this UiActor actor,
        Func<TViewData> initalData, params object[] parameters)
        where TDialog : IBaseDialog<TData, TViewData>
    {
        _dialogCoordinator ??= actor.ServiceProvider.GetRequiredService<IDialogCoordinator>();

        return async () =>
               {
                   TData result = await actor
                      .Dispatcher
                      .InvokeAsync(
                           () =>
                           {
                               TDialog dialog =
                                   parameters.Length == 0
                                       ? actor.ServiceProvider.GetRequiredService<TDialog>()
                                       : ActivatorUtilities.CreateInstance<TDialog>(actor.ServiceProvider, parameters);
                               var task = dialog.Init(initalData());

                               _dialogCoordinator.ShowDialog(dialog);

                               return task;
                           });

                   actor.Dispatcher.Post(() => _dialogCoordinator.HideDialog());

                   return result;
               };
    }

    public static DialogBuilder<TViewData> Dialog<TViewData>(
        this IObservable<TViewData> input, UiActor actor,
        params object[] parameters) => new(input, parameters, actor);

    [PublicAPI]
    public sealed class DialogBuilder<TInitialData>
    {
        private readonly UiActor _actor;
        private readonly IObservable<TInitialData> _data;
        private readonly object[] _parameters;

        public DialogBuilder(IObservable<TInitialData> data, object[] parameters, UiActor actor)
        {
            _data = data;
            _parameters = parameters;
            _actor = actor;
        }

        public IObservable<TResult> Of<TDialog, TResult>() where TDialog : IBaseDialog<TResult, TInitialData>
        {
            return _data.SelectMany(
                    data
                        => ShowDialog<TDialog, TResult, TInitialData>(_actor, () => data, _parameters)())
               .ObserveOnSelf();
        }

        public IObservable<TInitialData> Of<TDialog>()
            where TDialog : IBaseDialog<TInitialData, TInitialData>
            => Of<TDialog, TInitialData>();
    }
}