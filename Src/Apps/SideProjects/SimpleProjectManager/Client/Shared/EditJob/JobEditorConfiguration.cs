namespace SimpleProjectManager.Client.Shared.EditJob;

public sealed record JobEditorConfiguration(bool StatusEditing, bool SortOrderEditing)
{
    public static JobEditorConfiguration Default { get; } = new(false, false);

    public static JobEditorConfiguration NewJobConfig { get; } = new(true, true);

    public static JobEditorConfiguration EditJobConfig { get; } = new(true, false);
}