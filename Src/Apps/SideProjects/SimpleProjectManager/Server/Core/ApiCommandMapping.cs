using Akkatecture.Commands;

namespace SimpleProjectManager.Server.Core;

public sealed record ApiCommandMapping(Type TargetType, Func<object, ICommand> Converter)
{
    public static ApiCommandMapping For<TFrom>(Func<TFrom, ICommand> converter) => new(typeof(TFrom), Converter);
}