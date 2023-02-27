using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Akka.Actor;
using Akka.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager.Old;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class ProcessManagerActor : ReceiveActor
{
    private readonly IDisposable? _subscription;
    
    public ProcessManagerActor(IOptionsMonitor<ProcessConfiguration> configuration)
    {
        Self.Tell(configuration.CurrentValue);

        IActorRef self = Self;
        _subscription = configuration.OnChange(pc => self.Tell(pc));

        Receive<ProcessConfiguration>(NewConfiguration);
        Receive<Process[]>(SearchProcesses);
    }

    private void SearchProcesses(Process[] obj)
    {
        
    }

    private void NewConfiguration(ProcessConfiguration obj)
    {
        foreach (IActorRef actorRef in Context.GetChildren())
            Context.Stop(actorRef);

        DependencyResolver resolver = DependencyResolver.For(Context.System);

        Context.ActorOf(resolver.Props<ProcessFetcher>(), "Process_Fetcher");
    }

    protected override void PostStop() => _subscription?.Dispose();
}