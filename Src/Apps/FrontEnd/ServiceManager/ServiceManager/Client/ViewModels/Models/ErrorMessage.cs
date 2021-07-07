﻿using MudBlazor;

namespace ServiceManager.Client.ViewModels.Models
{
    public abstract record SnackbarMessage
    {
        public abstract void Apply(ISnackbar snackbar);
    }

    public sealed record SnackbarErrorMessage(string Message) : SnackbarMessage
    {
        public override void Apply(ISnackbar snackbar) 
            => snackbar.Add(Message, Severity.Error, options => options.SnackbarVariant = Variant.Filled);
    }
}