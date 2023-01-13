namespace SimpleProjectManager.Operation.Client.Setup;

public static class SetupExtensions
{
    public static int? MinusOneToNull(this int input)
        => input == -1 ? null : input;

    public static string? EmptyToNull(this string str)
        => string.IsNullOrWhiteSpace(str) ? null : str;
}