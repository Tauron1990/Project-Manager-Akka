using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class CaseSource<TValue, TResult> : PerlinkSource<TResult, ISourceBlock<TResult>>
    {
        private readonly Func<TValue> _selector;
        private readonly IDictionary<TValue, ISourceBlock<TResult>> _sources;
        private readonly ISourceBlock<TResult> _defaultSource;

        public CaseSource(Func<TValue> selector, IDictionary<TValue, ISourceBlock<TResult>> sources, ISourceBlock<TResult> defaultSource)
        {
            _selector = selector;
            _sources = sources;
            _defaultSource = defaultSource;
        }
        
        protected override void NextBlock(ISourceBlock<TResult> block)
        {
            
        }

        protected override ISourceBlock<TResult> NewBlock()
        {
            try
            {
                return _sources.TryGetValue(_selector(), out var result) ? result : _defaultSource;
            }
            catch (Exception e)
            {
                return Dataflow.Throw<TResult>(e);
            }
        }
    }
}