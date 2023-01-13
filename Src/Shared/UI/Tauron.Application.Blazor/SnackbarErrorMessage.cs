using MudBlazor;

namespace Tauron.Application.Blazor;

public sealed record SnackbarErrorMessage(string? Message) : SnackbarMessage
{
    public override void Apply(ISnackbar snackbar)
        => snackbar.Add(Message, Severity.Error, options => options.SnackbarVariant = Variant.Filled);
}