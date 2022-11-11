namespace Tauron.Features;

public interface IFeature : IResourceHolder
{
    IEnumerable<string> Identify();

    void PostStop();

    void PreStart();
}

public interface IFeature<TState> : IFeature
{
    void Init(IFeatureActor<TState> actor);
}