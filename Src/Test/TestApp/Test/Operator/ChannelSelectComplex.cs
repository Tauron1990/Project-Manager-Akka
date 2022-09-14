using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestApp.Test.Operator;

public abstract  class ChannelSelectComplex<TIn, TOut, TCollection> : ChannelOperatorBase<TIn, TOut>
{
    private readonly Func<TIn, TCollection, TOut> _resultSelector;
    private readonly Channel<(TIn Source, TCollection Item)> _dispatcher;
    
    protected ChannelSelectComplex(ChannelReader<TIn> reader, Func<TIn, TCollection, TOut> resultSelector)
        : base(reader)
    {
        _resultSelector = resultSelector;
        _dispatcher = Channel.CreateBounded<(TIn Source, TCollection Item)>(10);
        Reader.Completion.ContinueWith((_, s) => ((ChannelWriter<(TIn, TCollection)>)s!).Complete(), _dispatcher.Writer);
    }

    public override Task Completion => _dispatcher.Reader.Completion;

    public override int Count => _dispatcher.Reader.Count + base.Count;


    public override bool CanPeek => _dispatcher.Reader.CanPeek;

    public override async ValueTask<TOut> ReadAsync(CancellationToken cancellationToken = default)
        => _dispatcher.Reader.ReadAsync(cancellationToken);

    public override bool TryPeek(out TOut item)
        => base.TryPeek(out item);

    public override IAsyncEnumerable<TOut> ReadAllAsync(CancellationToken cancellationToken = new CancellationToken())
        => base.ReadAllAsync(cancellationToken);

    public override bool TryRead(out TOut item)
        => throw new System.NotImplementedException();

    public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = new CancellationToken())
        => throw new System.NotImplementedException();

    protected override bool CanCountCore { get; }
}