using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor.Internal;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public static class BaseDialogExtension
    {
        private static IDialogCoordinator? _dialogCoordinator;

        public static Func<Task<TData>> ShowDialog<TDialog, TData>(this UiActor actor, params Parameter[] parameters)
            where TDialog : IBaseDialog<TData, TData>
        {
            return ShowDialog<TDialog, TData, TData>(actor, () => default!, parameters);
        }

        public static Func<Task<TData>> ShowDialog<TDialog, TData>(this UiActor actor, Func<TData> initalData, params Parameter[] parameters)
            where TDialog : IBaseDialog<TData, TData>
        {
            return ShowDialog<TDialog, TData, TData>(actor, initalData, parameters);
        }

        public static Func<Task<TData>> ShowDialog<TDialog, TData, TViewData>(this UiActor actor, Func<TViewData> initalData, params Parameter[] parameters)
            where TDialog : IBaseDialog<TData, TViewData>
        {
            _dialogCoordinator ??= actor.LifetimeScope.Resolve<IDialogCoordinator>();

            return async () =>
                   {
                       var result = await actor
                                         .Dispatcher
                                         .InvokeAsync(() =>
                                                      {
                                                          var dialog = actor.LifetimeScope.Resolve<TDialog>(parameters);
                                                          var task = dialog.Init(initalData());

                                                          _dialogCoordinator.ShowDialog(dialog);
                                                          return task;
                                                      });

                       actor.Dispatcher.Post(() => _dialogCoordinator.HideDialog());

                       return result;
                   };
        }

        public static DialogBuilder<TViewData> Dialog<TViewData>(this IObservable<TViewData> input, UiActor actor, params Parameter[] parameters) => new(input, parameters, actor);

        [PublicAPI]
        public sealed class DialogBuilder<TInitialData>
        {
            private readonly IObservable<TInitialData> _data;
            private readonly Parameter[] _parameters;
            private readonly UiActor _actor;

            public DialogBuilder(IObservable<TInitialData> data, Parameter[] parameters, UiActor actor)
            {
                _data = data;
                _parameters = parameters;
                _actor = actor;
            }

            public IObservable<TResult> Of<TDialog, TResult>() where TDialog : IBaseDialog<TResult, TInitialData> 
                => _data.SelectMany(data => ShowDialog<TDialog, TResult, TInitialData>(_actor, () => data, _parameters)())
                        .ObserveOnSelf();

            public IObservable<TInitialData> Of<TDialog>() 
                where TDialog : IBaseDialog<TInitialData, TInitialData> => Of<TDialog, TInitialData>();

        }
    }
}