using System.Reactive.Linq;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public record struct ErrorData(bool IsOnline, CriticalError[] Errors);

public sealed class CriticalErrorsViewModel : BlazorViewModel
{
    public IObservable<ErrorData> Errors { get; }
    
    public CriticalErrorsViewModel(IStateFactory stateFactory, GlobalState globalState)
        : base(stateFactory)
        => Errors = globalState.IsOnline.CombineLatest(globalState.ErrorState.Errors, (online, errors) => new ErrorData(online, errors));
}