using System;
using System.Threading.Tasks.Dataflow;
using Akka.Util;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class AnySink<TInput> : SingleValueSink<TInput, bool, bool>
    {
        private readonly Func<TInput, bool>? _predicade;

        public AnySink(Func<TInput, bool>? predicade) 
            : base(false)
            => _predicade = predicade;

        protected override bool Evaluate(TInput input, Option<bool> current)
        {
            if(_predicade == null)
                return true;

            if (!_predicade(input)) return false;
            
            Cancel();
            return true;

        }

        protected override bool ToResult(bool data) => data;

        protected override void Connect(ISourceBlock<TInput> input, ITargetBlock<TInput> output)
        {
            if(_predicade == null)
                base.Connect(input, output);
            else
                input.LinkTo(output, new DataflowLinkOptions { MaxMessages = 1, PropagateCompletion = true });
        }
    }
}