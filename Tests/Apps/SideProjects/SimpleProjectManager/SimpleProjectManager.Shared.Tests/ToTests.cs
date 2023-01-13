using AutoFixture.Xunit2;

namespace SimpleProjectManager.Shared.Tests;

public sealed class ToTests
{
    [Theory]
    [AutoData]
    public async Task Value_Task_With_Result(string toTest)
    {
        string result = await To.VTask(() => toTest);

        Assert.Equal(toTest, result);
    }

    [Fact]
    public async Task Value_Task_With_Result_And_Exception()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.VTask<string>(() => throw new InvalidOperationException("Test Error")));

    [Theory]
    [AutoData]
    public async Task Task_With_Result(string toTest)
    {
        string result = await To.Task(() => toTest);

        Assert.Equal(toTest, result);
    }

    [Fact]
    public async Task Task_With_Result_And_Exception()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.Task<string>(() => throw new InvalidOperationException("Test Error")));

    [Fact]
    public async Task Value_Task_With_Action()
        => await To.VTaskV(() => { });

    [Fact]
    public async Task Value_Task_With_Action_And_Exception()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.VTaskV(() => throw new InvalidOperationException("Test Error")));

    [Fact]
    public async Task Task_With_Action()
        => await To.TaskV(() => { });

    [Fact]
    public async Task Task_With_Action_And_Exception()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.TaskV(() => throw new InvalidOperationException("Test Error")));
}