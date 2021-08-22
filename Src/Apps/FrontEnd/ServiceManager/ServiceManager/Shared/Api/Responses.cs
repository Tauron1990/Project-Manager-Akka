namespace ServiceManager.Shared.Api
{
    public sealed record StringApiContent(string Content);

    public sealed record ConfigOption(string Path, string DefaultValue);

    public sealed record ConfigOptionList(bool Error, string Message, ConfigOption[] Options);
}