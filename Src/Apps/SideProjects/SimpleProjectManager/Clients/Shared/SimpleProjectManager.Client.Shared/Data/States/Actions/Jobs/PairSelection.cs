using Vogen;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

[ValueObject(typeof(string))]
[Instance("Nothing", "")]
public readonly partial struct PairSelection
{
    public PairSelection Select(string? select)
        => string.IsNullOrWhiteSpace(select) ? Nothing : From(select);
}