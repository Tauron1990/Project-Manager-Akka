using MudBlazor;

namespace Tauron.Application.Blazor;

public sealed record SnackbarSuccessMessage(string Message) : SnackbarMessage
{
    public override void Apply(ISnackbar snackbar)
        => snackbar.Add(Message, Severity.Success, options => options.SnackbarVariant = Variant.Filled);
}