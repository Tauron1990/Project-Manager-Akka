using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestApp.Test.Operator;

public sealed class ChannelCastOperator<TFrom, TO> : ChannelOperatorBase<TFrom, TO>
{
    public ChannelCastOperator(ChannelReader<TFrom> reader) : base(reader) { }

    public override bool CanPeek => Reader.CanPeek;

    public override bool TryPeek([MaybeNullWhen(false)]out TO item)
    {
        if(!Reader.TryPeek(out var ele))
        {
            item = default!;

            return false;
        }

        item = (TO)(object)ele!;

        return true;
    }

    public override bool TryRead(out TO item)
    {
        if(!Reader.TryRead(out var ele))
        {
            item = default!;

            return false;
        }

        item = (TO)(object)ele!;

        return true;
    }

    public override async ValueTask<TO> ReadAsync(CancellationToken cancellationToken = default)
        => (TO)(object)(await Reader.ReadAsync(cancellationToken))!;

    public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        => Reader.WaitToReadAsync(cancellationToken);

    public override IAsyncEnumerable<TO> ReadAllAsync(CancellationToken cancellationToken = default)
        => Reader.ReadAllAsync(cancellationToken).Select(data => ((TO)(object)data!));

    protected override bool CanCountCore => true;
}