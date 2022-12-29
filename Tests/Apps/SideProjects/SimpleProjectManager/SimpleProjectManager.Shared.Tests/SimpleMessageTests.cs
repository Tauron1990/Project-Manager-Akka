using AutoFixture.Xunit2;

namespace SimpleProjectManager.Shared.Tests;

public sealed class SimpleMessageTests : StringVlaueTypeTester<SimpleMessage>
{
    [Theory]
    [AutoData]
    public override void Not_Null_Valid_Value(string value)
        => base.Not_Null_Valid_Value(value);

    [Fact]
    public override void Null_InValid_Value()
        => base.Null_InValid_Value();

    [Fact]
    public override void Empty_InValid_Value()
        => base.Empty_InValid_Value();

    [Fact]
    public override void Default_InValid_Value()
        => base.Default_InValid_Value();

    [Fact]
    public override void Empty_Equal_Value()
        => base.Empty_Equal_Value();
}