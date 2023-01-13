using System;
using System.Threading.Tasks;

namespace Tauron.Application.Blazor;

public sealed class RenderingManager
{
    private Func<Action, Task> _runAsync = action =>
                                           {
                                               action();

                                               return Task.CompletedTask;
                                           };

    private Action _stateHasChanged = () => { };

    public bool CanRender { get; private set; }


    public void Init(Action stateHasChanged, Func<Action, Task> runAsync)
    {
        _stateHasChanged = stateHasChanged;
        _runAsync = runAsync;
    }

    public void StateHasChanged()
    {
        CanRender = true;
        _stateHasChanged();
        CanRender = false;
    }

    public async Task StateHasChangedAsync()
    {
        CanRender = true;
        await _runAsync(_stateHasChanged).ConfigureAwait(false);
        CanRender = false;
    }

    //public async Task PerformTask(Action init, Action compled, Func<Task<bool>> run)
    //{
    //    init();
    //    await StateHasChangedAsync();
    //    if (await run())
    //    {
    //        compled();
    //        await StateHasChangedAsync();
    //    }
    //}

    //public Task PerformTask(Action init, Action compled, Func<Task> run)
    //    => PerformTask(
    //        init,
    //        compled,
    //        async () =>
    //        {
    //            await run();

    //            return true;
    //        });

    public async ValueTask PerformTask(Action init, Action compled, Func<ValueTask<bool>> run)
    {
        init();
        await StateHasChangedAsync().ConfigureAwait(false);

        await run().ConfigureAwait(false);

        compled();
        await StateHasChangedAsync().ConfigureAwait(false);
    }

    public async ValueTask PerformTask(Action init, Action compled, Func<ValueTask> run)
    {
        init();
        await StateHasChangedAsync().ConfigureAwait(false);

        await run().ConfigureAwait(false);

        compled();
        await StateHasChangedAsync().ConfigureAwait(false);
    }
}