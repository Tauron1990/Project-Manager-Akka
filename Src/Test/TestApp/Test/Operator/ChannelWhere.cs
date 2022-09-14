using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestApp.Test.Operator;

public sealed class ChannelWhere<TIn> : ChannelOperatorBase<TIn>
{
    private readonly Func<TIn, bool> _condition;

    public ChannelWhere(ChannelReader<TIn> reader, Func<TIn, bool> condition) 
        : base(reader)
        => _condition = condition;

    protected override bool CanCountCore => false;

    public override bool CanPeek => true;

    public override bool TryPeek([MaybeNullWhen(false)]out TIn item)
    {
        if(Reader.TryPeek(out var tempItem) && _condition(tempItem))
        {
            item = tempItem;

            return true;
        }

        item = default;

        return false;
    }

    public override async ValueTask<TIn> ReadAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var result = await Reader.ReadAsync(cancellationToken);

            if(_condition(result)) return result;
        }
    }

    public override IAsyncEnumerable<TIn> ReadAllAsync(CancellationToken cancellationToken = default)
        => Reader.ReadAllAsync(cancellationToken).Where(_condition);

    public override bool TryRead([MaybeNullWhen(false)] out TIn item)
    {
        if(Reader.TryRead(out var tempItem) && _condition(tempItem))
        {
            item = tempItem;

            return true;
        }

        item = default;

        return false;
    }

    public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        => Reader.WaitToReadAsync(cancellationToken);
}