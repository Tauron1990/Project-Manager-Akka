using MudBlazor;

namespace Tauron.Application.Blazor;

public abstract record SnackbarMessage
{
    public abstract void Apply(ISnackbar snackbar);
}