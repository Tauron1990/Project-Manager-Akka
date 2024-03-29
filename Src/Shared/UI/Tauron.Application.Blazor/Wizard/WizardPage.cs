﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor.Wizard;

[PublicAPI]
public abstract class WizardPage<TData> : IWizardPageBase, IDisposable
{
    public EventCallback BeforeNextEvent { get; set; }

    public virtual void Dispose()
    {
        BeforeNextEvent = default;
        GC.SuppressFinalize(this);
    }

    public virtual bool ShowControls { get; set; } = true;

    public abstract string Title { get; }

    public virtual IEnumerable<(string Label, Func<Task> Handler)> CustomActions { get; } = Array.Empty<(string, Func<Task>)>();

    Task<bool> IWizardPageBase.NeedRender(WizardContextBase context)
        => NeedRender((WizardContext<TData>)context);

    Task<Type> IWizardPageBase.Init(WizardContextBase context, CancellationToken token)
        => Init((WizardContext<TData>)context, token);

    public async Task<string?> VerifyNext(WizardContextBase context, CancellationToken token)
    {
        using var source = CancellationTokenSource.CreateLinkedTokenSource(token);
        source.CancelAfter(TimeSpan.FromSeconds(10));

        return await VerifyNextImpl((WizardContext<TData>)context, source.Token).ConfigureAwait(false);
    }

    public async Task BeforeNext(WizardContextBase context)
    {
        await BeforeNextEvent.InvokeAsync().ConfigureAwait(false);
        await OnBeforeNext((WizardContext<TData>)context).ConfigureAwait(false);
    }

    protected virtual Task OnBeforeNext(WizardContext<TData> data)
        => Task.CompletedTask;

    protected abstract Task<string?> VerifyNextImpl(WizardContext<TData> context, CancellationToken token);

    protected abstract Task<bool> NeedRender(WizardContext<TData> context);

    protected abstract Task<Type> Init(WizardContext<TData> context, CancellationToken cancellationToken);
}