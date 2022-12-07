using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.GamePackages.Elements;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public abstract class PackageElement
{
    internal abstract void Apply(IServiceCollection serviceCollection);

    internal abstract void PostConfig(IServiceProvider serviceProvider);
    
    public static PackageElement Group(IEnumerable<PackageElement> gamepackages)
        => new GroupingElement(gamepackages);

    public static PackageElement Translate(string path) => new Translator(path);

    public static PackageElement Asset(Action<AssetManager> configurator) => new AssetLoader(configurator);
    
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