using AutoFixture.Xunit2;

namespace SimpleProjectManager.Shared.Tests;

public sealed class ToTests
{
    [Theory, AutoData]
    public async Task ValueTaskWithResult(string toTest)
    {
        string result = await To.VTask(() => toTest);
        
        Assert.Equal(toTest, result);
    }
    
    [Fact]
    public async Task ValueTaskWithResultAndException()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.VTask<string>(() => throw new InvalidOperationException("Test Error")));
    
    [Theory, AutoData]
    public async Task TaskWithResult(string toTest)
    {
        string result = await To.Task(() => toTest);
        
        Assert.Equal(toTest, result);
    }
    
    [Fact]
    public async Task TaskWithResultAndException()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.Task<string>(() => throw new InvalidOperationException("Test Error")));

    [Fact]
    public async Task ValueTaskWithAction()
        => await To.VTaskV(() => {  });

    [Fact]
    public async Task ValueTaskWithActionAndException()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.VTaskV(() => throw new InvalidOperationException("Test Error")));
    
    [Fact]
    public async Task TaskWithAction()
        => await To.TaskV(() => { });

    [Fact]
    public async Task TaskWithActionAndException()
        => await Assert.ThrowsAsync<InvalidOperationException>(async () => await To.TaskV(() => throw new InvalidOperationException("Test Error")));
}