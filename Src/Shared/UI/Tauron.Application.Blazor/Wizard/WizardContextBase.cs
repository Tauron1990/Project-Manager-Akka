using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.Blazor.Wizard;

[PublicAPI]
public abstract class WizardContextBase
{
    protected WizardContextBase(ImmutableList<IWizardPageBase> pages)
        => Pages = pages;

    [PublicAPI]
    public ImmutableList<IWizardPageBase> Pages { get; }

    [PublicAPI]
    public int CurrentPosition { get; set; } = -1;

    public IWizardPageBase? CurrentPage { get; private set; }

    public static ProcessingWizardContext<TData, TResult> Create<TData, TResult>(TData data, Func<TData, Task<TResult>> processor, params WizardPage<TData>[] pages)
        => new(data, processor, pages.Cast<IWizardPageBase>().ToImmutableList());

    protected abstract Task WizardCompled();

    private async Task FinalizeWizard()
    {
        foreach (IDisposable page in Pages.OfType<IDisposable>())
            page.Dispose();

        await WizardCompled().ConfigureAwait(false);
    }

    internal async Task<Type?> Next(CancellationToken token)
    {
        IWizardPageBase page;

        do
        {
            CurrentPosition++;
            if(CurrentPosition >= Pages.Count)
            {
                CurrentPage = null;
                await FinalizeWizard().ConfigureAwait(false);

                return null;
            }

            page = Pages[CurrentPosition];
        } while (!await page.NeedRender(this).ConfigureAwait(false));

        CurrentPage = page;

        return await page.Init(this, token).ConfigureAwait(false);
    }

    internal async Task<Type?> Back(CancellationToken token)
    {
        IWizardPageBase page;

        if(CurrentPosition == 0) return null;

        do
        {
            CurrentPosition--;

            if(CurrentPosition == 0) return null;

            page = Pages[CurrentPosition];
        } while (!await page.NeedRender(this).ConfigureAwait(false));

        CurrentPage = page;

        return await page.Init(this, token).ConfigureAwait(false);
    }

    internal bool CanBack()
        => CurrentPosition < 0;

    internal bool CanNext()
        => CurrentPosition < Pages.Count;
}