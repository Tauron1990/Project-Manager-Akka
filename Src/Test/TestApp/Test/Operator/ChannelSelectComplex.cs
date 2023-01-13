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
        using var cancel = new CancellationTokenSource();
        // ReSharper disable once MethodSupportsCancellation
        #pragma warning disable CS4014
        // ReSharper disable once AccessToDisposedClosure
        Reader.Completion.ContinueWith(_ =>
                                       {
                                           try
                                           {
                                               cancel.Cancel();
                                           }
                                           catch (ObjectDisposedException)
                                           {
                                               
                                           }
                                       });
        #pragma warning restore CS4014
        
        try
        {
            await foreach (var source in Reader.ReadAllAsync(cancel.Token))
                await foreach (var element in SelectNext(source, cancel.Token))
                    await _dispatcher.Writer.WriteAsync((source, element), cancel.Token);
        }
        catch (Exception e)
        {
            if(e is OperationCanceledException)
                _dispatcher.Writer.TryComplete();
            else
                _dispatcher.Writer.TryComplete(e);
        }
    }
    
    protected abstract IAsyncEnumerable<TCollection> SelectNext(TIn input, CancellationToken token);

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

    public static ChannelReader<TOut> Create(ChannelReader<TIn> reader, Func<TIn, IEnumerable<TCollection>> collectionSelect, Func<TIn, TCollection, TOut> resultSelector)
        => new EnumerableSelect(reader, collectionSelect, resultSelector);

    private sealed class EnumerableSelect : ChannelSelectComplex<TIn, TOut, TCollection>
    {
        private readonly Func<TIn, IEnumerable<TCollection>> _collectionSelect;

        public EnumerableSelect(ChannelReader<TIn> reader, Func<TIn, IEnumerable<TCollection>> collectionSelect, Func<TIn, TCollection, TOut> resultSelector) 
            : base(reader, resultSelector)
            => _collectionSelect = collectionSelect;

        protected override IAsyncEnumerable<TCollection> SelectNext(TIn input, CancellationToken token)
            => _collectionSelect(input).ToAsyncEnumerable();
    }
    
    public static ChannelReader<TOut> Create(ChannelReader<TIn> reader, Func<TIn, Task<TCollection>> collectionSelect, Func<TIn, TCollection, TOut> resultSelector)
        => new TaskSelect(reader, collectionSelect, resultSelector);

    private sealed class TaskSelect : ChannelSelectComplex<TIn, TOut, TCollection>
    {
        private readonly Func<TIn, Task<TCollection>> _collectionSelect;

        public TaskSelect(ChannelReader<TIn> reader, Func<TIn, Task<TCollection>> collectionSelect, Func<TIn, TCollection, TOut> resultSelector) 
            : base(reader, resultSelector)
            => _collectionSelect = collectionSelect;

        protected override IAsyncEnumerable<TCollection> SelectNext(TIn input, CancellationToken token)
            => _collectionSelect(input).ToAsyncEnumerable();
    }
    
    public static ChannelReader<TOut> Create(ChannelReader<TIn> reader, Func<TIn, IObservable<TCollection>> collectionSelect, Func<TIn, TCollection, TOut> resultSelector)
        => new ObservableSelect(reader, collectionSelect, resultSelector);

    private sealed class ObservableSelect : ChannelSelectComplex<TIn, TOut, TCollection>
    {
        private readonly Func<TIn, IObservable<TCollection>> _collectionSelect;

        public ObservableSelect(ChannelReader<TIn> reader, Func<TIn, IObservable<TCollection>> collectionSelect, Func<TIn, TCollection, TOut> resultSelector) 
            : base(reader, resultSelector)
            => _collectionSelect = collectionSelect;

        protected override IAsyncEnumerable<TCollection> SelectNext(TIn input, CancellationToken token)
            => _collectionSelect(input).ToAsyncEnumerable();
    }
}