using System;
using Akka.Util;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class AllSink<TInput> : SingleValueSink<TInput, bool, bool>
    {
        private readonly Func<TInput, bool> _predicate;

        public AllSink(Func<TInput, bool> predicate)
            : base(Option<bool>.None)
            => _predicate = predicate;

        protected override bool Evaluate(TInput input, Option<bool> current)
        {
            if (current.HasValue && !current.Value)
                return false;
            return _predicate(input);
        }

        protected override bool ToResult(bool data) => data;
    }
}