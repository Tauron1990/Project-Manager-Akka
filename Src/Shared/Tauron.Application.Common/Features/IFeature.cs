using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Features
{
    public interface IFeature
    {
        void Init(IFeatureActorBase actorBase);
    }

    public interface IStatedFeature<TState> : IFeature
    {

    }

    [PublicAPI]
    public abstract class ActorFeatureBase : IFeature
    {
        void IFeature.Init(IFeatureActorBase actorBase) => Init((IFeatureActor)actorBase);

        protected abstract void Init(IFeatureActor actorBase);
    }

    [PublicAPI]
    public abstract class ActorFeatureBase<TState> : IStatedFeature<TState>
    {
        void IFeature.Init(IFeatureActorBase actorBase) => Init((IFeatureActor<TState>)actorBase);

        protected abstract void Init(IFeatureActor<TState> actorBase);
    }
}