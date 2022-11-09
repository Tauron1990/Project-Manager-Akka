using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Aggregates.Snapshot.Strategies;
using Akkatecture.Core;
using FluentValidation;
using FluentValidation.Results;
using SimpleProjectManager.Shared;
using Error = Tauron.Operations.Error;

namespace SimpleProjectManager.Server.Core.Data;

public interface ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot> : IHydrate<TSnapshot>
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity>
    where TIdentity : IIdentity
    where TAggregate : IAggregateRoot<TIdentity>
{
    TSnapshot CreateSnapshot();
}

public abstract class InternalState<TAggregate, TIdentity, TSnapshot> : AggregateState<TAggregate, TIdentity>, ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot>
    where TIdentity : IIdentity
    where TAggregate : IAggregateRoot<TIdentity>
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity>
{
    public abstract void Hydrate(TSnapshot aggregateSnapshot);
    public abstract TSnapshot CreateSnapshot();
}

public abstract class InternalAggregateRoot<TAggregate, TIdentity, TAggregateState, TSnapshot> : AggregateRoot<TAggregate, TIdentity, TAggregateState>
    where TAggregateState : AggregateState<TAggregate, TIdentity, IMessageApplier<TAggregate, TIdentity>>, ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot>
    where TIdentity : IIdentity
    where TAggregate : AggregateRoot<TAggregate, TIdentity, TAggregateState>
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity>
{
    protected InternalAggregateRoot(TIdentity id) : base(id, new AggregateRootSettings(TimeSpan.FromDays(7), true, true))
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        SetSnapshotStrategy(new SnapshotEveryFewVersionsStrategy(100));
    }

    protected bool Run<TCommand>(TCommand command, IValidator<TCommand>? validator, AggregateNeed need, Func<TCommand, IOperationResult> runner)
    {
        try
        {
            IOperationResult result;

            switch (need)
            {
                case AggregateNeed.New when !IsNew:
                    result = OperationResult.Failure(new Error(GetErrorMessage(AggregateError.NoNewError), AggregateError.NoNewError.Value));

                    break;
                case AggregateNeed.Exist when IsNew:
                    result = OperationResult.Failure(new Error(GetErrorMessage(AggregateError.NewError), AggregateError.NewError.Value));

                    break;
                case AggregateNeed.Nothing:
                default:
                    ValidationResult? validationResult = validator?.Validate(command);

                    result = !(validationResult?.IsValid ?? true)
                        ? CreateFailure(validationResult)
                        : runner(command);

                    break;
            }

            TellSenderIsPresent(result);

            return true;
        }
        catch (Exception e)
        {
            TellSenderIsPresent(OperationResult.Failure(e));
            Context.GetLogger().Error(e, "Eoor on process Command {Command}", command);

            return false;
        }
    }

    protected IOperationResult CreateFailure(ValidationResult result)
        => OperationResult.Failure(result.Errors.Select(err => new Error(err.ErrorMessage, err.ErrorCode)));

    protected void TellSenderIsPresent(object message)
    {
        if(Sender.IsNobody()) return;

        Sender.Tell(message);
    }

    protected override IAggregateSnapshot<TAggregate, TIdentity>? CreateSnapshot()
    {
        if(State == null) return null;

        return State.CreateSnapshot();
    }

    protected virtual string? GetErrorMessage(AggregateError errorCode)
        => null;

    protected static IValidator<TCarrier> CreateValidator<TCarrier, TData>(IValidator<TData> validator)
        where TCarrier : CommandCarrier<TData, TAggregate, TIdentity>
        => new InlineValidator<TCarrier>
           {
               v => v.RuleFor(c => c.Command).SetValidator(validator)
           };

    protected static IValidator<TCarrier> CreateEmptyValidator<TCarrier>()
        => new InlineValidator<TCarrier>();

    protected enum AggregateNeed
    {
        Nothing,
        New,
        Exist
    }
}