using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public sealed class MutationActor : ReceiveActor
    {
        public MutationActor()
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
        }

        //private sealed class HandlerTerminated
        //{
        //    public HandlerTerminated(Action remover)
        //    {
        //        Remover = remover;
        //    }

        //    public Action Remover { get; }
        //}
    }
}