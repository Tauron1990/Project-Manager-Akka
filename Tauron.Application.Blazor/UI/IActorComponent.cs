using System;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Tauron.Akka;
using Tauron.Application.CommonUI.Helper;

namespace Tauron.Application.Blazor.UI
{
    public interface IActorComponent : IComponent, IHandleEvent, IHandleAfterRender, IResourceHolder, IBinderControllable
    {
        public IObservable<Unit> Loaded { get; }

        public IObservable<Unit> Unloaded { get; }

        public Task InvokeAsync(Action action);

        public void StateHasChanged();
    }
}