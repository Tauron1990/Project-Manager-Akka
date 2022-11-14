using MudBlazor;

namespace Tauron.Application.Blazor;

public sealed record SnackbarWarningMessage(string Message) : SnackbarMessage
{
    public override void Apply(ISnackbar snackbar)
        => snackbar.Add(Message, Severity.Warning, options => options.SnackbarVariant = Variant.Filled);
}