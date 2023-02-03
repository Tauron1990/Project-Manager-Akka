namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record NewEntryChange(LocEntry NewEntry) : MutatingChange
    {
        public EntryAdd ToData() => new(NewEntry);
    }
}