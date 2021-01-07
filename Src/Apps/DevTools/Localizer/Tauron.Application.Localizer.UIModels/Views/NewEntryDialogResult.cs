namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed record NewEntryDialogResult(string Name);

    public abstract record NewEntryInfoBase;

    public sealed record NewEntryInfo(string Name) : NewEntryInfoBase;

    public sealed record NewEntrySuggestInfo(string Name) : NewEntryInfoBase;
}