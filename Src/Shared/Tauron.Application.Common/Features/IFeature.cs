using JetBrains.Annotations;

namespace Tauron.Features
{
    public interface IFeature<TState>
    {
        public void Init(IFeatureActor<TState> actor);
    }
    

    [PublicAPI]
    public abstract class ActorFeatureBase<TState> : IFeature<TState>
    {
        public abstract void Init(IFeatureActor<TState> actor);
    }
}