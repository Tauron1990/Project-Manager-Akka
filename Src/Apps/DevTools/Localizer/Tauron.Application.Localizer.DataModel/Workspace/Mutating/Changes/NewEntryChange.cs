using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record NewEntryChange(LocEntry NewEntry) : MutatingChange
    {
        public EntryAdd ToData() => new(NewEntry);
    }
}