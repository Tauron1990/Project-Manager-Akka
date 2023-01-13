using System.Collections.Immutable;

namespace SimpleProjectManager.Server.Data;

public static class Extensions
{
    public static async ValueTask<ImmutableList<TItem>> ToImmutableList<TItem>(this IAsyncEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        ImmutableList<TItem>.Builder builder = ImmutableList.CreateBuilder<TItem>();

        await foreach (TItem item in items.WithCancellation(cancellationToken).ConfigureAwait(false))
            builder.Add(item);

        return builder.ToImmutable();
    }
}