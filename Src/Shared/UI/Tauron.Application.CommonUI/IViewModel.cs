﻿using System;
using Akka.Actor;

namespace Tauron.Application.CommonUI;

public interface IViewModel
{
    IActorRef Actor { get; }

    Type ModelType { get; }

    bool IsInitialized { get; }

    public void AwaitInit(Action waiter);
}

// ReSharper disable once UnusedTypeParameter
public interface IViewModel<TModel> : IViewModel { }