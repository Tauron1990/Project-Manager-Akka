namespace SimpleProjectManager.Shared.Tests;

public abstract class StringVlaueTypeTester<TType>
    where TType : IStringValueType<TType>
{
    public virtual void NotNullValidValue(string value)
    {
        TType msg = TType.From(value);
        
        Assert.True(msg == value, "msg == value; operator");
        Assert.Equal(value, msg.Value);
    }

    public virtual void NullInValidValue()
        => Assert.Throws<Vogen.ValueObjectValidationException>(() => TType.From(null));

    public virtual void EmptyInValidValue()
        => Assert.Throws<Vogen.ValueObjectValidationException>(() => TType.From(string.Empty));
    
    public virtual void DefaultInValidValue()
        #pragma warning disable VOG009
        => Assert.Throws<Vogen.ValueObjectValidationException>(() => default(TType).Value);
    #pragma warning restore VOG009

    public virtual void EmptyEqualValue()
    {
        Assert.Equal(string.Empty, TType.GetEmpty.Value);
        Assert.True(TType.GetEmpty == string.Empty, "SimpleMessage.Empty == string.Empty; operator");
    }
}