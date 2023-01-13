using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.Blazor.Wizard;

[PublicAPI]
public sealed class ProcessingWizardContext<TData, TResult> : WizardContext<TData>
{
    private readonly Func<TData, Task<TResult>> _processor;

    public ProcessingWizardContext(TData data, Func<TData, Task<TResult>> processor, ImmutableList<IWizardPageBase> pages)
        : base(data, pages)
    {
        _processor = processor;
        Data = data;
    }

    #pragma warning disable MA0046
    public event Func<TResult, Task>? Finish;
    #pragma warning restore MA0046

    private Task OnFinish(TResult arg)
        => Finish?.Invoke(arg) ?? Task.CompletedTask;

    protected override async Task WizardCompled()
        => await OnFinish(await _processor(Data).ConfigureAwait(false)).ConfigureAwait(false);
}