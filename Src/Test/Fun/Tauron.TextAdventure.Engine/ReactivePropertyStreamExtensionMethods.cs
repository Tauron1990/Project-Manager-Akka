using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine;

[PublicAPI]
public static class ReactivePropertyStreamExtensionMethods
{
    public static ReactivePropertyStream<T> ToReactivePropertyStream<T>(this IObservable<T> stream)=> new ReactivePropertyStream<T>(stream);
        
}