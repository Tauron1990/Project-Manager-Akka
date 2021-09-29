using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager.Client.Shared.BaseComponents.Wizard
{
    public interface IWizardPageBase
    {
        bool ShowControls { get; set; }

        string Title { get; set; }
        
        IEnumerable<(string Label, Func<Task> Handler)> CustomActions { get; }
        
        Task<bool> NeedRender(WizardContextBase context);
        
        Task<Type> Init(WizardContextBase context, CancellationToken token);

        string? VerifyNext();
    }
    
    public abstract class WizardPage<TData> : IWizardPageBase
    {
        public virtual bool ShowControls { get; set; } = true;
        public abstract string Title { get; set; }

        public virtual IEnumerable<(string Label, Func<Task> Handler)> CustomActions { get; } = Array.Empty<(string, Func<Task>)>();

        Task<bool> IWizardPageBase.NeedRender(WizardContextBase context)
            => NeedRender((WizardContext<TData>)context);

        Task<Type> IWizardPageBase.Init(WizardContextBase context, CancellationToken token)
            => Init((WizardContext<TData>)context, token);

        public abstract string? VerifyNext();

        protected abstract Task<bool> NeedRender(WizardContext<TData> context);

        protected abstract Task<Type> Init(WizardContext<TData> context, CancellationToken cancellationToken);
    }
}