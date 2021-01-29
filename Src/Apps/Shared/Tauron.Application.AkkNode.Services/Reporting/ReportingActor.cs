using System;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Operations;

namespace Tauron.Application.AkkNode.Services
{
    public abstract class ReportingActor : ObservableActor
    {
        public static string GenralError = nameof(GenralError);

        [PublicAPI]
        protected void Receive<TMessage>(string name, Action<TMessage, Reporter> process) 
            where TMessage : IReporterMessage => Receive<TMessage>(obs => obs.Subscribe(m => TryExecute(m, name, process)));

        [PublicAPI]
        protected void ReceiveContinue<TMessage>(string name, Action<TMessage, Reporter> process)
            where TMessage : IDelegatingMessage => Receive<TMessage>(obs => obs.Subscribe(m => TryContinue(m, name, process)));

        protected virtual void TryExecute<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
            where TMessage : IReporterMessage
        {
            Log.Info("Enter Process {Name}", name);
            var reporter = Reporter.CreateReporter(Context);
            reporter.Listen(msg.Listner);

            try
            {
                process(msg, reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
                reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
            }
        }

        protected virtual void TryContinue<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
            where TMessage : IDelegatingMessage
        {
            Log.Info("Enter Process {Name}", name);
            try
            {
                process(msg, msg.Reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
                msg.Reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
            }
        }
    }
}