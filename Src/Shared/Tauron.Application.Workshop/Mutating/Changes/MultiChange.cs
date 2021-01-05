using System.Collections.Immutable;

namespace Tauron.Application.Workshop.Mutating.Changes
{
    public sealed record MultiChange(ImmutableList<MutatingChange> Changes) : MutatingChange
    {
        public override TChange Cast<TChange>()
        {
            foreach (var change in Changes)
            {
                if (change is TChange c)
                    return c;
            }

            return null!;
        }
    }
}