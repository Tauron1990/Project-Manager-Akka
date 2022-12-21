using AutoFixture.Xunit2;

namespace SimpleProjectManager.Shared.Tests;

public sealed class PropertyValueTests : StringVlaueTypeTester<PropertyValue>
{
    [Fact]
    public override void EmptyEqualValue()
        => base.EmptyEqualValue();

    [Fact]
    public override void DefaultInValidValue()
        => base.DefaultInValidValue();

    [Theory, AutoData]
    public override void NotNullValidValue(string value)
        => base.NotNullValidValue(value);

    [Fact]
    public override void EmptyInValidValue()
        => base.EmptyInValidValue();

    [Fact]
    public override void NullInValidValue()
        => base.NullInValidValue();
}