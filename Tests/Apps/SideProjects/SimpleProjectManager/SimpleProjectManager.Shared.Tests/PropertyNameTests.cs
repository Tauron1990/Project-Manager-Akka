using AutoFixture.Xunit2;

namespace SimpleProjectManager.Shared.Tests;

public sealed class PropertyNameTests : StringVlaueTypeTester<PropertyName>
{
    [Theory, AutoData]
    public override void NotNullValidValue(string value)
        => base.NotNullValidValue(value);

    [Fact]
    public override void EmptyEqualValue()
        => Assert.Throws<Vogen.ValueObjectValidationException>(() => base.EmptyEqualValue());

    [Fact]
    public override void DefaultInValidValue()
        => base.DefaultInValidValue();

    [Fact]
    public override void EmptyInValidValue()
        => base.EmptyInValidValue();

    [Fact]
    public override void NullInValidValue()
        => base.NullInValidValue();
}