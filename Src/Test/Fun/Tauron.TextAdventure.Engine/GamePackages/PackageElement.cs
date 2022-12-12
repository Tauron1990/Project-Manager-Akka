using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.GamePackages.Elements;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public abstract class PackageElement
{
    internal abstract void Apply(IServiceCollection serviceCollection);

    internal abstract void PostConfig(IServiceProvider serviceProvider);
    
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
    
    private sealed class GroupingElement : PackageElement
    {
        private readonly IEnumerable<PackageElement> _gamepackages;
        private HashSet<PackageElement>? _visited;

        public GroupingElement(IEnumerable<PackageElement> gamepackages)
            => _gamepackages = gamepackages;


        internal override void Apply(IServiceCollection serviceCollection)
            => Run(serviceCollection, (element, collection) => element.Apply(collection));

        internal override void PostConfig(IServiceProvider serviceProvider)
            => Run(serviceProvider, (element, provider) => element.PostConfig(provider));

        private void Run<TParm>(TParm parm, Action<PackageElement, TParm> runner)
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

                    runner(element, parm);
                }
                
            } while (!list.IsEmpty);

            _visited = null;
        }
    }
}