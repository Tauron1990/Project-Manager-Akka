using Akka.Actor;
using Tauron.AkkaHost;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Analyzing.Actor;
using Tauron.Application.Workshop.Core;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Driver;

public class AkkaDriverFactory : IDriverFactory
{
    private Func<Props, Props>? Config { get; init; }

    public AkkaDriverFactory CustomMutator(Func<Props, Props>? configurator) 
        => new() { Config = configurator };    
    
    public Action<IDataMutation> CreateMutator()
    {
        var superviser = WorkspaceSuperviser.Get(ActorApplication.ActorSystem);
        var props = Props.Create(() => new MutationActor());
        if (Config is not null)
            props = Config(props);
        
        var actor = new DeferredActor(superviser.Create(props, "Mutator"));

        return m => actor.TellToActor(m);
    }

    public Action<RegisterRule<TWorkspace, TData>> CreateAnalyser<TWorkspace, TData>(TWorkspace workspace, IObserver<RuleIssuesChanged<TWorkspace, TData>> observer) where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        var superviser = WorkspaceSuperviser.Get(ActorApplication.ActorSystem);
        var actor = new DeferredActor(superviser.Create(Props.Create(() => new AnalyzerActor<TWorkspace, TData>(workspace, observer)), "Analyser"));

        return r => actor.TellToActor(r);
    }
}