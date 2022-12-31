namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public sealed record JobEditorConfiguration(bool StatusEditing, bool SortOrderEditing)
{
    public static JobEditorConfiguration Default { get; } = new(StatusEditing: false, SortOrderEditing: false);

    public static JobEditorConfiguration NewJobConfig { get; } = new(StatusEditing: true, SortOrderEditing: true);

    public static JobEditorConfiguration EditJobConfig { get; } = new(StatusEditing: true, SortOrderEditing: false);
}