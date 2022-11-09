using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor.Wizard;

public abstract class WizardPageComponent<TData, TPage> : ComponentBase
{
    [Parameter]
    public WizardContext<TData> Context { get; set; } = null!;

    [Parameter]
    public IWizardPageBase InternalPage { get; set; } = null!;

    public TPage Page => (TPage)InternalPage;

    [Parameter]
    public EventCallback OnNext { get; set; }
}