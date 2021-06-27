using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.Blazor
{
    public abstract class GenericDialogBase<TResult, TInput> : ComponentBase, IBaseDialog<TResult, TInput>
    {
        [CascadingParameter] 
        protected MudDialogInstance? MudDialog { get; set; }

        [Parameter]
        public TInput? InitialData { get; set; }

        public GenericDialogBase()
        {
            
        }

        public Task<TResult> Init(TInput initalData) 
            => Show().Do(_ => { }, _ => MudDialog?.Cancel(), () => MudDialog?.Close()).ToTask();

        protected abstract IObservable<TResult> Show();
    }
}