using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentValidation;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.ObservableExt;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class Reducer<TAction, TData> : IReducer<TData>
        where TData : IStateEntity
        where TAction : IStateAction
    {
        public virtual IValidator<TAction>? Validator { get; }

        public virtual Func<IObservable<MutatingContext<TData>>, IObservable<ReducerResult<TData>>> Reduce(
            IStateAction action)
        {
            static IObservable<ReducerResult<TData>> ErrorHandler(Exception e)
            {
                return Observable.Return(new ReducerResult<TData>(null, new[] {e.Message}));
            }

            return state =>
            {
                var typedAction = (TAction) action;
                var validation = Validator?.Validate(typedAction);

                return
                    state.ConditionalSelect()
                        .ToResult<ReducerResult<TData>>(b =>
                        {
                            b.When(_ => validation != null && !validation.IsValid,
                                context => context.Select(c
                                    => ReducerResult.Fail<TData>(validation!.Errors.Select(f => f.ErrorMessage))));
                            b.When(_ => validation == null || validation.IsValid,
                                context =>
                                    Reduce(context.Where(_ => validation == null || validation.IsValid), typedAction)
                                        .Catch<ReducerResult<TData>, Exception>(ErrorHandler));
                        });
            };
        }

        public virtual bool ShouldReduceStateForAction(IStateAction action) => action is TAction;

        protected abstract IObservable<ReducerResult<TData>> Reduce(IObservable<MutatingContext<TData>> state,
            TAction action);

        protected ReducerResult<TData> Sucess(MutatingContext<TData> data) => ReducerResult.Sucess(data);

        protected ReducerResult<TData> Fail(MutatingContext<TData> data, IEnumerable<string> errors)
            => ReducerResult.Fail<TData>(errors);

        protected Task<ReducerResult<TData>> SucessAsync(MutatingContext<TData> data)
            => Task.FromResult(ReducerResult.Sucess(data));

        protected Task<ReducerResult<TData>> FailAsync(MutatingContext<TData> data, IEnumerable<string> errors)
            => Task.FromResult(ReducerResult.Fail<TData>(errors));
    }
}