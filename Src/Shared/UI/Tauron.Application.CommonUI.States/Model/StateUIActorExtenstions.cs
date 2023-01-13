using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.CommonUI.Model;

[PublicAPI]
public static class StateUIActorExtenstions
{
    public static CommandRegistrationBuilder ToStateAction<TStateAction>(this CommandRegistrationBuilder builder)
        where TStateAction : IStateAction, new()
    {
        return ToStateAction(builder, _ => new TStateAction());
    }

    public static CommandRegistrationBuilder ToStateAction(
        this CommandRegistrationBuilder builder,
        Func<IStateAction?> action)
    {
        return ToStateAction(builder, _ => action());
    }

    public static CommandRegistrationBuilder ToStateAction<TParameter>(
        this CommandRegistrationBuilder builder,
        Func<TParameter, IStateAction?> action)
    {
        return ToStateAction(
            builder,
            o =>
            {
                if(o is TParameter parameter)
                    return action(parameter);

                return action(default!);
            });
    }

    public static CommandRegistrationBuilder ToStateAction(
        this CommandRegistrationBuilder builder,
        Func<object?, IStateAction?> action)
    {
        StateUIActor invoker = TryCast(builder);

        return builder.WithExecute(
            o =>
            {
                IStateAction? stateAction = action(o);

                if(stateAction is null) return;

                invoker.DispatchAction(stateAction);
            });
    }

    private static StateUIActor TryCast(CommandRegistrationBuilder builder)
    {
        if(builder.Target is StateUIActor uiActor)
            return uiActor;

        throw new InvalidOperationException("command Builder is not a State Actor");
    }
}