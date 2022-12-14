using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.GamePackages.Core;
using Tauron.TextAdventure.Engine.GamePackages.Elements;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public abstract class PackageElement
{
    internal abstract void Load(ElementLoadContext context);
    
    public static PackageElement Group(IEnumerable<PackageElement> gamepackages)
        => new GroupingElement(gamepackages);

    public static PackageElement Translate(IHostEnvironment environment, string path) => new Translator(path, environment);

    public static PackageElement Asset(Action<AssetManager> configurator) => new AssetLoader(configurator);

    public static PackageElement System<TSystem>()
        where TSystem : class, ISystem
        => new SystemElement<TSystem>();

    public static PackageElement Event<TEvent>(Action<GameState, TEvent> apply) 
        where TEvent : IEvent
        => new RegisterEvent<TEvent>(apply);

    public static PackageElement Init(Action<GameState> init)
        => new InitGame(init);

    public static PackageElement ModifyRoom<TBuilder>(string name, Action<TBuilder> builder) 
        where TBuilder : RoomBuilderBase, new()
        => new RoomModifyElement<TBuilder>(name, builder);

    public static PackageElement NewRoom<TBuilder>(string name, Action<TBuilder> builder)
        where TBuilder : RoomBuilderBase, new()
        => new RoomElement<TBuilder>(name, builder);
    
    private sealed class GroupingElement : PackageElement
    {
        private readonly IEnumerable<PackageElement> _gamepackages;
        private HashSet<PackageElement>? _visited;

        public GroupingElement(IEnumerable<PackageElement> gamepackages)
            => _gamepackages = gamepackages;

        internal override void Load(ElementLoadContext context)
        {
            var list = ImmutableQueue<IEnumerable<PackageElement>>.Empty.Enqueue(_gamepackages);
            _visited = new HashSet<PackageElement>();
            
            do
            {
                list = list.Dequeue(out var toProcess);

                foreach (PackageElement element in toProcess)
                {
                    if(!_visited.Add(element))
                        continue;
                    
                    if(element is GroupingElement group)
                    {
                        list = list.Enqueue(group._gamepackages);
                        continue;
                    }

                    element.Load(context);
                }
                
            } while (!list.IsEmpty);

            _visited = null;
        }
    }
}