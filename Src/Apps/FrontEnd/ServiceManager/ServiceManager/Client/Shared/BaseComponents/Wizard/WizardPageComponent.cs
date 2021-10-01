using System.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace ServiceManager.Client.Shared.BaseComponents.Wizard
{
    public abstract class WizardPageComponent<TData, TPage> : Component
    {
        [Parameter]
        public WizardContext<TData> Context { get; set; } = null!;

        [Parameter] 
        public IWizardPageBase InternalPage { get; set; } = null!;

        public TPage Page => (TPage)InternalPage;
        
        [Parameter]
        public EventCallback OnNext { get; set; }
    }
}