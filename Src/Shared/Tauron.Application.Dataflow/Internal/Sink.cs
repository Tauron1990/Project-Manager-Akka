using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal abstract class Sink<TInput, TOutput>
    {
        protected abstract IPropagatorBlock<TInput, TOutput> CreateBlock(ISourceBlock<TInput> input);

        protected virtual void Connect(ISourceBlock<TInput> input, ITargetBlock<TInput> output)
            => input.LinkTo(output, SourceBlockExtensions.DefaultAnonymosLinkOptions);

        public ISourceBlock<TOutput> LinkTo(ISourceBlock<TInput> input)
        {
            var output = CreateBlock(input);
            Connect(input, output);
            return output;
        }
    }
}