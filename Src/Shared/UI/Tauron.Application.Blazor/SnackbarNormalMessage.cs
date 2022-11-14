using MudBlazor;

namespace Tauron.Application.Blazor;

public sealed record SnackbarNormalMessage(string Message) : SnackbarMessage
{
    public override void Apply(ISnackbar snackbar)
        => snackbar.Add(Message, Severity.Normal, options => options.SnackbarVariant = Variant.Filled);
}