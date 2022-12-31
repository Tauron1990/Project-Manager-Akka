using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.Model;
using Tauron.TAkka;

namespace Tauron.Application.CommonUI;

[PublicAPI]
public static class UiActorExtensions
{
    public static ModelProperty RegisterModel<TModel>(this UiActor actor, string propertyName, string actorName)
    {
        var model = actor.ServiceProvider.GetRequiredService<IViewModel<TModel>>();
        model.InitModel(ObservableActor.ExposedContext, actorName);

        return new ModelProperty(model.Actor, actor.RegisterProperty<IViewModel<TModel>>(propertyName).WithDefaultValue(model).Property.LockSet());
    }

    public static UIProperty<TData> RegisterImport<TData>(this UiActor actor, string propertyName)
        where TData : notnull
    {
        var target = actor.ServiceProvider.GetRequiredService<TData>();

        return (UIProperty<TData>)actor.RegisterProperty<TData>(propertyName).WithDefaultValue(target).Property
           .LockSet();
    }
}