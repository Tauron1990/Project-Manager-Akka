using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher.WorkDistributor
{
    public sealed class WorkDistributorDispatcher : ActorBase
    {
        public sealed record MutationCompled
        {
            public static readonly MutationCompled Inst = new();
        }

        [PublicAPI]
        public sealed class DistMutationActor : ReceiveActor
        {
            public DistMutationActor()
            {
                Receive<ISyncMutation>(Mutation);
                ReceiveAsync<IAsyncMutation>(Mutation);
            }

            private static ILoggingAdapter Log => Context.GetLogger();

            private async Task Mutation(IAsyncMutation mutation)
            {
                try
                {
                    Log.Info("Mutation Begin: {Name}", mutation.Name);
                    await mutation.Run();
                    Log.Info("Mutation Finisht: {Name}", mutation.Name);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Mutation Failed: {Name}", mutation.Name);
                }
                finally
                {
                    Context.Parent.Tell(MutationCompled.Inst);
                }
            }

            private void Mutation(ISyncMutation obj)
            {
                try
                {
                    Log.Info("Mutation Begin: {Name}", obj.Name);
                    obj.Run();
                    Log.Info("Mutation Finisht: {Name}", obj.Name);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Mutation Failed: {Name}", obj.Name);
                }
                finally
                {
                    Context.Parent.Tell(MutationCompled.Inst);
                }
            }
        }

        private readonly IWorkDistributor<IDataMutation> _mutator;

        public WorkDistributorDispatcher(TimeSpan timeout) 
            => _mutator = WorkDistributorFeature<IDataMutation, MutationCompled>.Create(Context, Props.Create<DistMutationActor>(), "Worker", timeout);

        protected override bool Receive(object message)
        {
            if (message is IDataMutation mutation)
            {
                _mutator.PushWork(mutation);
                return true;
            }
            return false;
        }
    }
}