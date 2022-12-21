using AutoFixture.Xunit2;
using FluentAssertions;
using SimpleProjectManager.Shared.Tests.TestData;
using Tauron.Operations;
using Vogen;

namespace SimpleProjectManager.Shared.Tests;

public sealed class ResultExtensionsTests
{
    [Theory, AutoData]
    public void ThrowWhenNotEmptyString(string value)
    {
        Action act = value.ThrowIfFail;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DontThrowWhenEmptyString()
    {
        var empty = string.Empty;
        string? nullValue = null;

        empty.ThrowIfFail();
        nullValue.ThrowIfFail();
    }
    
    [Theory, AutoData]
    public void ThrowWhenNotEmptyStringWithResult(string value)
    {
        var act = () => value.ThrowIfFail(() => 1);
        
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DontThrowWhenEmptyStringWithResult()
    {
        var empty = string.Empty;
        string? nullValue = null;

        int result1 = empty.ThrowIfFail(() => 1);
        int result2 = nullValue.ThrowIfFail(() => 1);

        result1.Should().Be(1);
        result2.Should().Be(1);
    }
    
    
    
    [Theory, DomainAutoData]
    public void ThrowWhenNotEmptySimpleResult(SimpleResult value)
    {
        Action act = () => value.ThrowIfFail();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DontThrowWhenEmptySimpleResult()
    {
        SimpleResult empty = SimpleResult.Success();
        SimpleResult defaultValue = default;

        empty.ThrowIfFail();
        defaultValue.ThrowIfFail();
    }
    
    [Theory, DomainAutoData]
    public void ThrowWhenNotEmptySimpleResultWithResult(SimpleResult value)
    {
        var act = () => value.ThrowIfFail(() => 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DontThrowWhenEmptySimpleResultWithResult()
    {
        var empty = string.Empty;
        string? defaultValue = null;

        int result1 = empty.ThrowIfFail(() => 1);
        int result2 = defaultValue.ThrowIfFail(() => 1);

        result1.Should().Be(1);
        result2.Should().Be(1);
    }

    [Fact]
    public void ValidateNullOrEmptyString()
    {
        const string nameContent = nameof(ValidateNullOrEmptyString);
        
        string? nullValue = null;
        var emptyValue = string.Empty;

        Validation nullResult = nullValue.ValidateNotNullOrEmpty(nameContent);
        Validation emptyResult = emptyValue.ValidateNotNullOrEmpty(nameContent);

        nullResult.Should().NotBeNull();
        nullResult.ErrorMessage
           .Should().NotBeNullOrWhiteSpace()
           .And.Contain(nameContent, Exactly.Once());

        emptyResult.Should().NotBeNull();
        emptyResult.ErrorMessage
           .Should().NotBeNullOrWhiteSpace()
           .And.Contain(nameContent, Exactly.Once());
    }

    [Theory, AutoData]
    public void ValidateValidString(string value)
    {
        Validation result = value.ValidateNotNullOrEmpty(nameof(ValidateValidString));

        result.Should().NotBeNull();
        result.ErrorMessage.Should().BeEmpty();
    }
}