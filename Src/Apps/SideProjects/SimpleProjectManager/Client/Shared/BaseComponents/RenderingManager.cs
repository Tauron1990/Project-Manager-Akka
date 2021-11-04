namespace SimpleProjectManager.Client.Shared.BaseComponents;

public sealed class RenderingManager
{
    private Action _stateHasChanged = () => {};
    private Func<Action, Task> _runAsync = action =>
                                           {
                                               action();
                                               return Task.CompletedTask;
                                           };

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
        await _runAsync(_stateHasChanged);
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
        await StateHasChangedAsync();

        await run();

        compled();
        await StateHasChangedAsync();
    }

    public async ValueTask PerformTask(Action init, Action compled, Func<ValueTask> run)
    {
        init();
        await StateHasChangedAsync();

        await run();

        compled();
        await StateHasChangedAsync();
    }
}