﻿using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Wpf.Model
{
    public sealed class ViewModelActorRef<TModel> : DefaultActorRef<TModel>, IViewModel<TModel>
        where TModel : ActorBase
    {
        public ViewModelActorRef(ActorRefFactory<TModel> actorBuilder) 
            : base(actorBuilder)
        {
        }

        public Type ModelType => typeof(TModel);
    }
}