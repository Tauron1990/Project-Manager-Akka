using MudBlazor;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Data.Converter;

public static class ProjectNameConverter
{
    public static Converter<ProjectName> Inst { get; } = new()
    {
        GetFunc = FromString,
        SetFunc = ConvertToString,
    };

    private static string? ConvertToString(ProjectName? arg)
        => arg?.Value;

    private static ProjectName? FromString(string? arg) =>
        string.IsNullOrWhiteSpace(arg) ? null : new ProjectName(arg);
}