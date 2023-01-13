namespace Tauron.Features;

public sealed record EmptyState
{
    public static readonly EmptyState Inst = new();
}