using MudBlazor;

namespace SimpleProjectManager.Client
{
    public abstract record SnackbarMessage
    {
        public abstract void Apply(ISnackbar snackbar);
    }

    public sealed record SnackbarErrorMessage(string? Message) : SnackbarMessage
    {
        public override void Apply(ISnackbar snackbar)
            => snackbar.Add(Message, Severity.Error, options => options.SnackbarVariant = Variant.Filled);
    }

    public sealed record SnackbarWarningMessage(string Message) : SnackbarMessage
    {
        public override void Apply(ISnackbar snackbar)
            => snackbar.Add(Message, Severity.Warning, options => options.SnackbarVariant = Variant.Filled);
    }

    public sealed record SnackbarSuccessMessage(string Message) : SnackbarMessage
    {
        public override void Apply(ISnackbar snackbar)
            => snackbar.Add(Message, Severity.Success, options => options.SnackbarVariant = Variant.Filled);
    }

    public sealed record SnackbarNormalMessage(string Message) : SnackbarMessage
    {
        public override void Apply(ISnackbar snackbar)
            => snackbar.Add(Message, Severity.Normal, options => options.SnackbarVariant = Variant.Filled);
    }

    public sealed record SnackbarInfoMessage(string Message) : SnackbarMessage
    {
        public override void Apply(ISnackbar snackbar)
            => snackbar.Add(Message, Severity.Info, options => options.SnackbarVariant = Variant.Filled);
    }
}