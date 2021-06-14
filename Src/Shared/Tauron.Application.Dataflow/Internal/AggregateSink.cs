using System;
using Akka.Util;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class AggregateSink<TSource, TAccumulate, TResult> : SingleValueSink<TSource, TAccumulate, TResult>
    {
        private readonly Func<TAccumulate, TSource, TAccumulate> _accumulator;
        private readonly Func<TAccumulate, TResult> _resultSelector;

        public AggregateSink(TAccumulate? current, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector) 
            : base(current ?? Option<TAccumulate>.None)
        {
            _accumulator = accumulator;
            _resultSelector = resultSelector;
        }

        protected override TAccumulate Evaluate(TSource input, Option<TAccumulate> current) => _accumulator(current.GetOrElse(default!), input);

        protected override TResult ToResult(TAccumulate data) => _resultSelector(data);
    }
}