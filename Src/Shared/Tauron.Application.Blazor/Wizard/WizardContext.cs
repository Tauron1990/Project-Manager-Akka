using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.Blazor.Wizard;

public abstract class WizardContextBase
{
    public static ProcessingWizardContext<TData, TResult> Create<TData, TResult>(TData data, Func<TData, Task<TResult>> processor, params WizardPage<TData>[] pages)
        => new(data, processor, pages.Cast<IWizardPageBase>().ToImmutableList());

    [PublicAPI]
    public ImmutableList<IWizardPageBase> Pages { get; }

    [PublicAPI]
    public int CurrentPosition { get; set; } = -1;

    public IWizardPageBase? CurrentPage { get; set; }
        
    protected WizardContextBase(ImmutableList<IWizardPageBase> pages)
        => Pages = pages;

    protected abstract Task WizardCompled();

    private async Task FinalizeWizard()
    {
        foreach (var page in Pages.OfType<IDisposable>()) 
            page.Dispose();

        await WizardCompled();
    }
        
    internal async Task<Type?> Next(CancellationToken token)
    {
        IWizardPageBase page;
            
        do
        {
            CurrentPosition++;
            if (CurrentPosition >= Pages.Count)
            {
                CurrentPage = null;
                await FinalizeWizard();

                return null;
            }

            page = Pages[CurrentPosition];
        } while (!await page.NeedRender(this));

        CurrentPage = page;
        return await page.Init(this, token);
    }
        
    internal async Task<Type?> Back(CancellationToken token)
    {
        IWizardPageBase page;

        if (CurrentPosition == 0) return null;
            
        do
        {
            CurrentPosition--;
            if (CurrentPosition == 0) return null;
                
            page = Pages[CurrentPosition];
        } while (!await page.NeedRender(this));

        CurrentPage = page;
        return await page.Init(this, token);
    }

    internal bool CanBack()
        => CurrentPosition < 0;

    internal bool CanNext()
        => CurrentPosition < Pages.Count;
}
    
public abstract class WizardContext<TData> : WizardContextBase
{
        
    [PublicAPI]
    public TData Data { get; set; }
        
    protected WizardContext(TData data, ImmutableList<IWizardPageBase> pages) : base(pages)
        => Data = data;
}

public sealed class ProcessingWizardContext<TData, TResult> : WizardContext<TData>
{
        
    private readonly Func<TData, Task<TResult>> _processor;

    public event Func<TResult, Task>? Finish;

    public ProcessingWizardContext(TData data, Func<TData, Task<TResult>> processor, ImmutableList<IWizardPageBase> pages) 
        : base(data, pages)
    {
        _processor = processor;
        Data = data;
    }

    private Task OnFinish(TResult arg)
        => Finish?.Invoke(arg) ?? Task.CompletedTask;

    protected override async Task WizardCompled()
        => await OnFinish(await _processor(Data));
}