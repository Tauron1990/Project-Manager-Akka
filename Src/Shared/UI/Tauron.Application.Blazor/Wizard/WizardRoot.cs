﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor.Wizard;

public partial class WizardRoot
{
    private static readonly RenderFragment<(Type? Type, Func<Task> Next, object Reciever)> RenderPage =
        dat => b =>
               {
                   (Type? comp, var callback, object reciever) = dat;

                   if(comp == null)
                       return;

                   b.OpenComponent(0, comp);
                   b.AddAttribute(1, "OnNext", EventCallback.Factory.Create(reciever, callback));
                   b.CloseComponent();
               };

    private readonly CancellationTokenSource _mainSource = new();

    private (Type? Page, CancellationTokenSource? Source) _currentPage;

    private bool _loading = true;

    [Parameter]
    public WizardContextBase? Context { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    public void Dispose()
    {
        _mainSource.Cancel();
        _mainSource.Dispose();
    }

    protected override Task OnInitializedAsync() => NextCallback();

    private async Task BackCallback()
    {
        if(Context == null) return;

        _currentPage.Source?.Dispose();
        var newSource = CancellationTokenSource.CreateLinkedTokenSource(_mainSource.Token);

        try
        {
            _currentPage = (await Context.Back(newSource.Token), newSource);
        }
        catch
        {
            newSource.Dispose();

            throw;
        }
    }

    private async Task NextCallback()
    {
        if(Context == null) return;

        _loading = true;
        StateHasChanged();

        await (Context.CurrentPage?.BeforeNext(Context) ?? Task.CompletedTask);

        string? err = Context.CurrentPage == null ? null : await Context.CurrentPage.VerifyNext(Context, _mainSource.Token);
        if(!string.IsNullOrWhiteSpace(err))
        {
            _aggregator.PublishWarnig($"Fehler: {err}");

            return;
        }

        _currentPage.Source?.Dispose();
        var newSource = CancellationTokenSource.CreateLinkedTokenSource(_mainSource.Token);

        try
        {
            _currentPage = (await Context.Next(newSource.Token), newSource);
        }
        catch
        {
            newSource.Dispose();

            throw;
        }

        _loading = false;
        StateHasChanged();
    }

    private Task CancelCallback()
    {
        _mainSource.Cancel();

        return OnCancel.InvokeAsync();
    }

    private bool CanBack()
        => _loading || Context?.CanNext() == true;

    private bool CanNext()
        => _loading || Context?.CanNext() == true;
}