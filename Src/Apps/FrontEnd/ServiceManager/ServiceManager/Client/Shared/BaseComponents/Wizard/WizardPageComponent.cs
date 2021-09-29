using System.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace ServiceManager.Client.Shared.BaseComponents.Wizard
{
    public abstract class WizardPageComponent<TData> : Component
    {
        [Parameter]
        public WizardContext<TData> Context { get; set; } = null!;

        [Parameter] 
        public WizardPage<TData> Page { get; set; } = null!;
        
        [Parameter]
        public EventCallback OnNext { get; set; }
    }
}