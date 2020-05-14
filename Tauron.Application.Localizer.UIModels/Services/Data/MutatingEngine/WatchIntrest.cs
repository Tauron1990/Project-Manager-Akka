﻿using System;
using Akka.Actor;

namespace Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine
{
    public sealed class WatchIntrest
    {
        public Action OnRemove { get; }

        public IActorRef Target { get; }

        public WatchIntrest(Action onRemove, IActorRef target)
        {
            OnRemove = onRemove;
            Target = target;
        }
    }
}