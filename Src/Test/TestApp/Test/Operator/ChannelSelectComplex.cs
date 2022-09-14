using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestApp.Test.Operator;

public abstract class ChannelSelectComplex<TIn, TOut, TCollection> : ChannelOperatorBase<TIn, TOut>
{
    private readonly Func<TIn, TCollection, TOut> _resultSelector;
    private readonly Channel<(TIn Source, TCollection Item)> _dispatcher;
    
    protected ChannelSelectComplex(ChannelReader<TIn> reader, Func<TIn, TCollection, TOut> resultSelector)
        : base(reader)
    {
        _resultSelector = resultSelector;
        _dispatcher = Channel.CreateBounded<(TIn Source, TCollection Item)>(10);
        Reader.Completion.ContinueWith((_, s) => ((ChannelWriter<(TIn, TCollection)>)s!).TryComplete(), _dispatcher.Writer);
        Task.Run(Processor);
    }

    public override Task Completion => _dispatcher.Reader.Completion;

    public override int Count => _dispatcher.Reader.Count + base.Count;


    public override bool CanPeek => _dispatcher.Reader.CanPeek;

    private async void Processor()
    {
        try
        {
            await foreach(var source in Reader.ReadAllAsync())
        }
        catch (Exception e)
        {
            _dispatcher.Writer.TryComplete(e);
        }
    }
    
    protected abstract IAsyncEnumerable<TCollection> SelectNext(TIn input);

    public override async ValueTask<TOut> ReadAsync(CancellationToken cancellationToken = default)
    {
        var next = await _dispatcher.Reader.ReadAsync(cancellationToken);

        return _resultSelector(next.Source, next.Item);
    }

    public override bool TryPeek([MaybeNullWhen(false)]out TOut item)
    {
        if(!_dispatcher.Reader.TryPeek(out var next))
        {
            item = default;

            return false;
        }

        item = _resultSelector(next.Source, next.Item);

        return true;
    }

    public override IAsyncEnumerable<TOut> ReadAllAsync(CancellationToken cancellationToken = default)
        => _dispatcher.Reader.ReadAllAsync(cancellationToken).Select(next => _resultSelector(next.Source, next.Item));

    public override bool TryRead([MaybeNullWhen(false)]out TOut item)
    {
        if(!_dispatcher.Reader.TryRead(out var next))
        {
            item = default;

            return false;
        }

        item = _resultSelector(next.Source, next.Item);

        return true;
    }

    public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        => _dispatcher.Reader.WaitToReadAsync(cancellationToken);

    protected override bool CanCountCore => true;
}