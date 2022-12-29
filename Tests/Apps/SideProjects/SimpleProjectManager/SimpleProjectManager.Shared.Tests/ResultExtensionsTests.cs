using AutoFixture.Xunit2;
using FluentAssertions;
using SimpleProjectManager.Shared.Tests.TestData;
using Tauron.Operations;
using Vogen;

namespace SimpleProjectManager.Shared.Tests;

public sealed class ResultExtensionsTests
{
    [Theory]
    [AutoData]
    public void Throw_When_Not_Empty_String(string value)
    {
        Action act = value.ThrowIfFail;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Dont_Throw_When_Empty_String()
    {
        var empty = string.Empty;
        string? nullValue = null;

        empty.ThrowIfFail();
        nullValue.ThrowIfFail();
    }

    [Theory]
    [AutoData]
    public void Throw_When_Not_Empty_String_With_Result(string value)
    {
        var act = () => value.ThrowIfFail(() => 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Dont_Throw_When_Empty_String_With_Result()
    {
        var empty = string.Empty;
        string? nullValue = null;

        int result1 = empty.ThrowIfFail(() => 1);
        int result2 = nullValue.ThrowIfFail(() => 1);

        result1.Should().Be(1);
        result2.Should().Be(1);
    }


    [Theory]
    [DomainAutoData]
    public void Throw_When_Not_Empty_Simple_Result(SimpleResult value)
    {
        Action act = () => value.ThrowIfFail();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Dont_Throw_When_Empty_Simple_Result()
    {
        SimpleResult empty = SimpleResult.Success();
        SimpleResult defaultValue = default;

        empty.ThrowIfFail();
        defaultValue.ThrowIfFail();
    }

    [Theory]
    [DomainAutoData]
    public void Throw_When_Not_Empty_Simple_Result_With_Result(SimpleResult value)
    {
        var act = () => value.ThrowIfFail(() => 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Dont_Throw_When_Empty_Simple_Result_With_Result()
    {
        SimpleResult empty = SimpleResult.Success();
        SimpleResult defaultValue = default;

        int result1 = empty.ThrowIfFail(() => 1);
        int result2 = defaultValue.ThrowIfFail(() => 1);

        result1.Should().Be(1);
        result2.Should().Be(1);
    }

    [Fact]
    public void Validate_Null_Or_Empty_String()
    {
        const string nameContent = nameof(Validate_Null_Or_Empty_String);

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

    [Theory]
    [AutoData]
    public void Validate_Valid_String(string value)
    {
        Validation result = value.ValidateNotNullOrEmpty(nameof(Validate_Valid_String));

        result.Should().NotBeNull();
        result.ErrorMessage.Should().BeEmpty();
    }
}