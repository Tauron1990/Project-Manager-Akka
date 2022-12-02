using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public abstract class PackageElement
{
    internal abstract void Apply(IServiceCollection serviceCollection);

    public static PackageElement Group(IEnumerable<PackageElement> gamepackages)
        => new GroupingElement(gamepackages);

    private sealed class GroupingElement : PackageElement
    {
        private readonly IEnumerable<PackageElement> _gamepackages;
        private HashSet<PackageElement>? _visited;

        public GroupingElement(IEnumerable<PackageElement> gamepackages)
            => _gamepackages = gamepackages;

        internal override void Apply(IServiceCollection serviceCollection)
        {
            var list = ImmutableQueue<IEnumerable<PackageElement>>.Empty.Enqueue(_gamepackages);
            _visited = new HashSet<PackageElement>();
            
            do
            {
                list = list.Dequeue(out var toProcess);

                foreach (var element in toProcess)
                {
                    if(!_visited.Add(element))
                        continue;
                    
                    if(element is GroupingElement group)
                    {
                        list = list.Enqueue(group._gamepackages);
                        continue;
                    }
                    
                    element.Apply(serviceCollection);
                }
                
            } while (!list.IsEmpty);

            _visited = null;
        }
    }
}