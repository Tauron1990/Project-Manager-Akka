﻿using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public sealed record HookEvent(Delegate Invoker, Type Target)
{
    public static HookEvent Create<TType>(Action<TType> action) => new(action, typeof(TType));
}