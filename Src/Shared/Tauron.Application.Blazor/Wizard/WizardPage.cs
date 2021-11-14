using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor.Wizard
{
    public interface IWizardPageBase
    {
        bool ShowControls { get; set; }

        string Title { get; }
        
        IEnumerable<(string Label, Func<Task> Handler)> CustomActions { get; }
        
        Task<bool> NeedRender(WizardContextBase context);
        
        Task<Type> Init(WizardContextBase context, CancellationToken token);

        Task<string?> VerifyNext(WizardContextBase context, CancellationToken token);

        Task BeforeNext(WizardContextBase context);
    }
    
    public abstract class WizardPage<TData> : IWizardPageBase, IDisposable
    {
        public virtual bool ShowControls { get; set; } = true;
        
        public abstract string Title { get; }

        public EventCallback BeforeNextEvent { get; set; }
        
        public virtual IEnumerable<(string Label, Func<Task> Handler)> CustomActions { get; } = Array.Empty<(string, Func<Task>)>();

        Task<bool> IWizardPageBase.NeedRender(WizardContextBase context)
            => NeedRender((WizardContext<TData>)context);

        Task<Type> IWizardPageBase.Init(WizardContextBase context, CancellationToken token)
            => Init((WizardContext<TData>)context, token);

        public async Task<string?> VerifyNext(WizardContextBase context, CancellationToken token)
        {
            using var source = CancellationTokenSource.CreateLinkedTokenSource(token);
            source.CancelAfter(TimeSpan.FromSeconds(10));

            return await VerifyNextImpl((WizardContext<TData>)context, source.Token);
        }

        public async Task BeforeNext(WizardContextBase context)
        {
            await BeforeNextEvent.InvokeAsync();
            await OnBeforeNext((WizardContext<TData>)context);
        }

        public virtual Task OnBeforeNext(WizardContext<TData> data)
            => Task.CompletedTask;

        protected abstract Task<string?> VerifyNextImpl(WizardContext<TData> context,CancellationToken token);

        protected abstract Task<bool> NeedRender(WizardContext<TData> context);

        protected abstract Task<Type> Init(WizardContext<TData> context, CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            BeforeNextEvent = default;
            GC.SuppressFinalize(this);
        }
    }
}