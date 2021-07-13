namespace ServiceManager.Shared.Api
{
    public sealed record StringApiContent(string Content);

    public sealed record BoolApiContent(bool Content);

    public sealed record ConfigOption(string Path, string DefaultValue);

    public sealed record ConfigOptionList(ConfigOption[] Options);
}