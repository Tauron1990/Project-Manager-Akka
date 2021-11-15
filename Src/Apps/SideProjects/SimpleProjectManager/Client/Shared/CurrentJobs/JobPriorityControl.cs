using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared.Services;
using Tauron;
using Tauron.Application.Blazor;
using System.Reactive.Disposables;
using SimpleProjectManager.Client.ViewModels;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobPriorityControl
{
    
    [Parameter]
    public JobsViewModel? Model { get; set; }

    [Parameter]
    public ImmutableList<JobSortOrderPair> ActivePairs { get; set; } = ImmutableList<JobSortOrderPair>.Empty;

    private IDisposable _currentSubsrciption = Disposable.Empty;
    private JobSortOrderPair? _info;
    private bool _canGoUp;
    private bool _canGoDown;
    private bool _canPriority;

    private bool CanNotGoUp => _processing || !_canGoUp;
    private bool CanNotGoDown => _processing || !_canGoDown;
    private bool CanNotPriority => _processing || !_canPriority;

    private bool _processing;

    protected override void OnParametersSet()
    {
        RemoveResource(_currentSubsrciption);

        _currentSubsrciption = 
            Model?.CurrentInfo
            .Subscribe(NewJobIncomming)
            .DisposeWith(this) ?? Disposable.Empty;

        base.OnParametersSet();
    }

    private async Task GoUp()
    {
        if(_info == null) return;
        var index = ActivePairs.IndexOf(_info) - 1;
        if(index == -1) return;

        await RenderingManager.PerformTask(
            () => _processing = true,
            () => _processing = false,
            () => _aggregator.IsSuccess(() => TimeoutToken.WithDefault(
                token => _jobDatabase.ChangeOrder(new SetSortOrder(_info.Order.Increment()), token))));
    }

    private async Task GoDown()
    {
        if (_info == null) return;

        var index = ActivePairs.IndexOf(_info) + 1;
        if (index == ActivePairs.Count) return;

        await RenderingManager.PerformTask(
            () => _processing = true,
            () => _processing = false,
            () => _aggregator.IsSuccess(() => TimeoutToken.WithDefault(
                token => _jobDatabase.ChangeOrder(new SetSortOrder(_info.Order.Decrement()), token))));
    }

    private async Task Priorize()
    {
        if(_info == null || _info.Order.IsPriority) return;

        await RenderingManager.PerformTask(
            () => _processing = true,
            () => _processing = false,
            () => _aggregator.IsSuccess(() => TimeoutToken.WithDefault(
                token => _jobDatabase.ChangeOrder(new SetSortOrder(_info.Order.Priority()), token))));
    }

    private void NewJobIncomming(JobSortOrderPair? info)
    {
        void Invalid()
        {
            _canPriority = false;
            _canGoUp = false;
            _canGoDown = false;
        }

        _info = info;

        if (info != null && ActivePairs.Count != 0)
        {
            _canPriority = !info.Order.IsPriority;
            
            if (!ActivePairs.Contains(info))
                Invalid();
            else
            {
                _canGoUp = ActivePairs[0] != info;
                _canGoDown = ActivePairs.Last() != info;
            }
            
        }
        else
            Invalid();

        RenderingManager.StateHasChanged();
    }
}