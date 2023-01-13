using System;
using Akka.Actor;
using Akka.DependencyInjection;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.CommonUI.Helper;

public sealed class ViewModelSuperviser
{
    private static ViewModelSuperviser? _superviser;


    private readonly IActorRef _coordinator;

    private ViewModelSuperviser(IActorRef coordinator) => _coordinator = coordinator;


    public static ViewModelSuperviser Get(ActorSystem system)
    {
        return _superviser ??= new ViewModelSuperviser(system.ActorOf(DependencyResolver.For(system).Props<ViewModelSuperviserActor>(), nameof(ViewModelSuperviser)));
    }

    public void Create(IViewModel model, string? name = null)
    {
        if(model is ViewModelActorRef actualModel)
            _coordinator.Tell(new CreateModel(actualModel, name));
        else
            throw new InvalidOperationException($"Model mot Compatible with {nameof(ViewModelActorRef)}");
    }

    internal sealed class CreateModel
    {
        internal CreateModel(ViewModelActorRef model, string? name)
        {
            Model = model;
            Name = name;
        }

        internal ViewModelActorRef Model { get; }

        internal string? Name { get; }
    }
}