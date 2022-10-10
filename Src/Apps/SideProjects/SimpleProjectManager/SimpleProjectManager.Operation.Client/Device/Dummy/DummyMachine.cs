namespace SimpleProjectManager.Operation.Client.Device.Dummy;

public class DummyMachine : IMachine
{
    public Task Init()
        => Task.CompletedTask;
}