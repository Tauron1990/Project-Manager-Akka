using MudBlazor;

namespace Tauron.Application.Blazor;

public sealed record SnackbarInfoMessage(string Message) : SnackbarMessage
{
    public override void Apply(ISnackbar snackbar)
        => snackbar.Add(Message, Severity.Info, options => options.SnackbarVariant = Variant.Filled);
}