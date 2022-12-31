using Akka.Actor;

namespace Tauron.Application.CommonUI.Helper;

public static class ViewModelSuperviserExtensions
{
    public static void InitModel(this IViewModel model, IActorContext context, string? name = null)
        => ViewModelSuperviser.Get(context.System).Create(model, name);
}