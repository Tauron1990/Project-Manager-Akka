using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.CommonUI;

[PublicAPI]
public sealed class ModelProperty
{
    public ModelProperty(IActorRef model, UIPropertyBase property)
    {
        Model = model;
        Property = property;
    }

    public IActorRef Model { get; }

    public UIPropertyBase Property { get; }
}