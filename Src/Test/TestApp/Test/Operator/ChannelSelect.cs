using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestApp.Test.Operator;

public sealed class ChannelSelect<TIn, TOut> : ChannelOperatorBase<TIn, TOut>
{
    private readonly Func<TIn, TOut> _transform;

    public ChannelSelect(ChannelReader<TIn> reader, Func<TIn, TOut> transform)  
        : base(reader)
    {
        _transform = transform;
    }

    public override Task Completion => Reader.Completion;
    protected override bool CanCountCore => true;

    public override bool CanPeek => true;

    public override bool TryPeek([MaybeNullWhen(false)]out TOut item)
    {
        if(!Reader.TryPeek(out var input))
        {
            item = default;
            return false;
        }

        item = _transform(input);

        return true;
    }

    public override IAsyncEnumerable<TOut> ReadAllAsync(CancellationToken cancellationToken = default)
        => Reader.ReadAllAsync(cancellationToken).Select(data => _transform(data));

    public override async ValueTask<TOut> ReadAsync(CancellationToken cancellationToken = default)
        => _transform(await Reader.ReadAsync(cancellationToken));

    public override bool TryRead([MaybeNullWhen(false)]out TOut item)
    {
        if(!Reader.TryRead(out var input))
        {
            item = default;

            return false;
        }

        item = _transform(input);

        return true;
    }

    public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        => Reader.WaitToReadAsync(cancellationToken);
}