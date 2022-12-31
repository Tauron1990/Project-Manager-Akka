using System;

namespace Tauron.Application.Blazor.Commands;

public sealed record CommandState(Action Execute, bool Disabled)
{
    public static readonly CommandState Default = new(() => { }, Disabled: true);
}