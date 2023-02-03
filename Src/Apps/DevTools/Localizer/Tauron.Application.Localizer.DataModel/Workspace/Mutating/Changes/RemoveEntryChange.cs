namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record RemoveEntryChange(LocEntry Entry) : MutatingChange;
}