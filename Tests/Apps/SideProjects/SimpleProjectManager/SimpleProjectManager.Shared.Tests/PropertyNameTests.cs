using AutoFixture.Xunit2;
using Vogen;

namespace SimpleProjectManager.Shared.Tests;

public sealed class PropertyNameTests : StringVlaueTypeTester<PropertyName>
{
    [Theory]
    [AutoData]
    public override void Not_Null_Valid_Value(string value)
        => base.Not_Null_Valid_Value(value);

    [Fact]
    public override void Empty_Equal_Value()
        => Assert.Throws<ValueObjectValidationException>(() => base.Empty_Equal_Value());

    [Fact]
    public override void Default_InValid_Value()
        => base.Default_InValid_Value();

    [Fact]
    public override void Empty_InValid_Value()
        => base.Empty_InValid_Value();

    [Fact]
    public override void Null_InValid_Value()
        => base.Null_InValid_Value();
}