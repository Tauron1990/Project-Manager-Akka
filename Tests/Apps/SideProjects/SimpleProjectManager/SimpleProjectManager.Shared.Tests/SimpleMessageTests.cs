using AutoFixture.Xunit2;

namespace SimpleProjectManager.Shared.Tests;

public sealed class SimpleMessageTests : StringVlaueTypeTester<SimpleMessage>
{
    [Theory, AutoData]
    public override void NotNullValidValue(string value)
        => base.NotNullValidValue(value);

    [Fact]
    public override void NullInValidValue()
        => base.NullInValidValue();

    [Fact]
    public override void EmptyInValidValue()
        => base.EmptyInValidValue();
    
    [Fact]
    public override void DefaultInValidValue()
        => base.DefaultInValidValue();

    [Fact]
    public override void EmptyEqualValue()
        => base.EmptyEqualValue();
}