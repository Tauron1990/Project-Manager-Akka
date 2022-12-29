using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

namespace Tauron.TextAdventure.Engine.GamePackages.Core;

internal class ElementLoadContext
{
    internal readonly List<Action<IServiceCollection>> ConfigServices = new();
    internal readonly List<Action<IServiceProvider>> PostConfigServices = new();
    internal readonly Dictionary<string, Action<RoomBuilderBase>> RoomModify = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, Func<RoomBuilderBase>> Rooms = new(StringComparer.Ordinal);
}