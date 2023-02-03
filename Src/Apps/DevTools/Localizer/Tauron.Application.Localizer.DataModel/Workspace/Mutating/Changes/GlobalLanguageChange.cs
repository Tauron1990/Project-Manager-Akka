namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record GlobalLanguageChange(ActiveLanguage Language) : MutatingChange;
}